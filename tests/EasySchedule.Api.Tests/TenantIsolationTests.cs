using System.Net;
using System.Net.Http.Json;
using EasySchedule.Api.Endpoints;
using EasySchedule.Domain.Entities;
using EasySchedule.Domain.Enums;
using EasySchedule.Infrastructure.Security;
using EasySchedule.Infrastructure.Tenancy;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace EasySchedule.Api.Tests;

/// <summary>SPEC ข้อ 8: โรงเรียน A มองไม่เห็นข้อมูลโรงเรียน B</summary>
[Collection(ApiCollection.Name)]
public sealed class TenantIsolationTests(ApiFixture fx) : IAsyncLifetime
{
    private int _schoolA;
    private int _schoolB;
    private int _termB;
    private int _levelB;
    private string _userB = "";

    public async Task InitializeAsync()
    {
        _schoolA = await fx.SampleSchoolIdAsync();
        await using var db = fx.CreateDb(SystemTenantContext.Instance);
        var suffix = Guid.NewGuid().ToString("N")[..6];

        var school = new School { SchCode = "B" + suffix, Name = "โรงเรียนบีทดสอบ" };
        var term = new Term { School = school, Year = 2569, TermNo = 1, IsActive = true };
        var level = new ClassLevel { School = school, Term = term, Code = 22, Name = "ระดับลับของโรงเรียน B" };
        db.ClassLevels.Add(level);
        db.Teachers.Add(new Teacher { School = school, Term = term, Code = "TB1", Name = "ครู B01" });

        _userB = "adminB" + suffix;
        var user = new User { Username = _userB, DisplayName = "ฝ่ายวิชาการ B", Role = UserRole.SchoolAdmin, School = school, PasswordHash = "" };
        user.PasswordHash = new PasswordService().Hash(user, ApiFixture.SeedPassword);
        db.Users.Add(user);
        await db.SaveChangesAsync();
        (_schoolB, _termB, _levelB) = (school.Id, term.Id, level.Id);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Theory]
    [InlineData("admin")]
    [InlineData("viewer")]
    public async Task School_A_users_cannot_read_school_B_via_api(string username)
    {
        var clientA = await fx.LoginAsync(username);
        var res = await clientA.GetAsync($"/api/schools/{_schoolB}/classlevels");
        res.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task School_A_admin_cannot_write_into_school_B_via_api()
    {
        var clientA = await fx.LoginAsync("admin");
        var res = await clientA.PostAsJsonAsync($"/api/schools/{_schoolB}/classlevels", new CreateClassLevelRequest(23, "แอบเขียน", 1, 8, null));
        res.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        await using var db = fx.CreateDb(SystemTenantContext.Instance);
        (await db.ClassLevels.AnyAsync(l => l.SchoolId == _schoolB && l.Code == 23)).Should().BeFalse();
    }

    [Fact]
    public async Task School_A_admin_cannot_use_school_B_term_id()
    {
        var clientA = await fx.LoginAsync("admin");
        var res = await clientA.GetAsync($"/api/schools/{_schoolA}/classlevels?termId={_termB}");
        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task School_B_admin_sees_only_own_data_and_not_school_A()
    {
        var clientB = await fx.LoginAsync(_userB);
        var own = await clientB.GetFromJsonAsync<List<ClassLevelDto>>($"/api/schools/{_schoolB}/classlevels");
        own.Should().ContainSingle().Which.Name.Should().Be("ระดับลับของโรงเรียน B");

        var other = await clientB.GetAsync($"/api/schools/{_schoolA}/classlevels");
        other.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task SuperAdmin_can_read_any_school()
    {
        var client = await fx.LoginAsync("superadmin");
        var levels = await client.GetFromJsonAsync<List<ClassLevelDto>>($"/api/schools/{_schoolB}/classlevels");
        levels.Should().Contain(l => l.Code == 22);
    }

    [Fact]
    public async Task Global_query_filter_hides_other_school_rows_even_when_queried_directly()
    {
        await using var dbA = fx.CreateDb(new FixedTenantContext(_schoolA));

        (await dbA.ClassLevels.Where(l => l.SchoolId == _schoolB).ToListAsync()).Should().BeEmpty();
        (await dbA.ClassLevels.FirstOrDefaultAsync(l => l.Id == _levelB)).Should().BeNull();
        (await dbA.Teachers.Where(t => t.SchoolId == _schoolB).ToListAsync()).Should().BeEmpty();
        (await dbA.Terms.AnyAsync(t => t.Id == _termB)).Should().BeFalse();
        (await dbA.Schools.ToListAsync()).Should().NotContain(s => s.Id == _schoolB);
        (await dbA.ClassLevels.ToListAsync()).Should().NotBeEmpty().And.OnlyContain(l => l.SchoolId == _schoolA);
    }

    [Fact]
    public async Task Unknown_tenant_sees_nothing()
    {
        await using var db = fx.CreateDb(new FixedTenantContext(null));
        (await db.ClassLevels.ToListAsync()).Should().BeEmpty();
        (await db.Schools.ToListAsync()).Should().BeEmpty();
    }

    [Fact]
    public async Task Writing_a_row_for_another_school_is_rejected()
    {
        await using var dbA = fx.CreateDb(new FixedTenantContext(_schoolA));
        dbA.Teachers.Add(new Teacher { SchoolId = _schoolB, TermId = _termB, Code = "HACK", Name = "แอบใส่" });
        var act = () => dbA.SaveChangesAsync();
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task New_rows_get_current_school_id_automatically()
    {
        await using var dbB = fx.CreateDb(new FixedTenantContext(_schoolB));
        var room = new Room { TermId = _termB, Code = "R" + Guid.NewGuid().ToString("N")[..5], Name = "ห้องใหม่" };
        dbB.Rooms.Add(room);
        await dbB.SaveChangesAsync();
        room.SchoolId.Should().Be(_schoolB);
    }
}
