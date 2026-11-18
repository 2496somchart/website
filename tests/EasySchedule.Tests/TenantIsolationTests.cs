using System.Net;
using System.Net.Http.Json;
using EasySchedule.Api.Endpoints;
using EasySchedule.Domain.Entities;
using EasySchedule.Domain.Enums;
using EasySchedule.Infrastructure.Persistence;
using EasySchedule.Infrastructure.Security;
using EasySchedule.Infrastructure.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace EasySchedule.Tests;

[Collection(ApiCollection.Name)]
public sealed class TenantIsolationTests(ApiFixture fx) : IAsyncLifetime
{
    private int _schoolA;
    private int _schoolB;
    private int _levelB;
    private string _userB = "";

    /// <summary>สร้างโรงเรียน B (ข้อมูลของตัวเอง + ผู้ดูแล) แยกจากโรงเรียนตัวอย่าง A</summary>
    public async Task InitializeAsync()
    {
        _schoolA = await fx.SampleSchoolIdAsync();
        await using var db = fx.CreateDb(SystemTenantContext.Instance);
        var suffix = Guid.NewGuid().ToString("N")[..6];

        var school = new School { Code = "B" + suffix, Name = "โรงเรียนบีทดสอบ" };
        db.Schools.Add(school);
        await db.SaveChangesAsync();
        _schoolB = school.Id;

        var mode = new ScheduleMode { SchoolId = _schoolB, Name = "ตาราง B" };
        var level = new ClassLevel { SchoolId = _schoolB, Code = "SECRET", Name = "ระดับลับของโรงเรียน B", ScheduleMode = mode };
        db.ClassLevels.Add(level);
        db.Teachers.Add(new Teacher { SchoolId = _schoolB, Code = "TB1", FirstName = "ครูบี", LastName = "ทดสอบ" });

        _userB = "adminB" + suffix;
        var user = new User { Username = _userB, DisplayName = "ผู้ดูแล B", Role = UserRole.SchoolAdmin, SchoolId = _schoolB, PasswordHash = "" };
        user.PasswordHash = new PasswordService().Hash(user, ApiFixture.SeedPassword);
        db.Users.Add(user);
        await db.SaveChangesAsync();
        _levelB = level.Id;
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task School_A_admin_cannot_read_school_B_classlevels_via_api()
    {
        var clientA = await fx.LoginAsync("admin");
        var res = await clientA.GetAsync($"/api/schools/{_schoolB}/classlevels");
        Assert.Equal(HttpStatusCode.Forbidden, res.StatusCode);
    }

    [Fact]
    public async Task School_A_viewer_cannot_read_school_B_classlevels_via_api()
    {
        var clientA = await fx.LoginAsync("viewer");
        var res = await clientA.GetAsync($"/api/schools/{_schoolB}/classlevels");
        Assert.Equal(HttpStatusCode.Forbidden, res.StatusCode);
    }

    [Fact]
    public async Task School_A_admin_cannot_write_into_school_B_via_api()
    {
        var clientA = await fx.LoginAsync("admin");
        var res = await clientA.PostAsJsonAsync($"/api/schools/{_schoolB}/classlevels", new CreateClassLevelRequest("X1", "แอบเขียน", 1, null));
        Assert.Equal(HttpStatusCode.Forbidden, res.StatusCode);

        await using var db = fx.CreateDb(SystemTenantContext.Instance);
        Assert.False(await db.ClassLevels.AnyAsync(l => l.SchoolId == _schoolB && l.Code == "X1"));
    }

    [Fact]
    public async Task School_B_admin_sees_only_own_data_and_not_school_A()
    {
        var clientB = await fx.LoginAsync(_userB);
        var own = await clientB.GetFromJsonAsync<List<ClassLevelDto>>($"/api/schools/{_schoolB}/classlevels");
        var only = Assert.Single(own!);
        Assert.Equal("ระดับลับของโรงเรียน B", only.Name);

        var other = await clientB.GetAsync($"/api/schools/{_schoolA}/classlevels");
        Assert.Equal(HttpStatusCode.Forbidden, other.StatusCode);
    }

    [Fact]
    public async Task SuperAdmin_can_read_any_school()
    {
        var client = await fx.LoginAsync("superadmin");
        var levels = await client.GetFromJsonAsync<List<ClassLevelDto>>($"/api/schools/{_schoolB}/classlevels");
        Assert.Contains(levels!, l => l.Code == "SECRET");
    }

    [Fact]
    public async Task Global_query_filter_hides_other_school_rows_even_when_queried_directly()
    {
        await using var dbA = fx.CreateDb(new FixedTenantContext(_schoolA));

        // ถึงจะ where SchoolId == B ตรง ๆ ก็ต้องไม่เห็น
        Assert.Empty(await dbA.ClassLevels.Where(l => l.SchoolId == _schoolB).ToListAsync());
        Assert.Null(await dbA.ClassLevels.FirstOrDefaultAsync(l => l.Id == _levelB));
        Assert.Empty(await dbA.Teachers.Where(t => t.SchoolId == _schoolB).ToListAsync());
        Assert.DoesNotContain(await dbA.Schools.ToListAsync(), s => s.Id == _schoolB);
        Assert.All(await dbA.ClassLevels.ToListAsync(), l => Assert.Equal(_schoolA, l.SchoolId));
        Assert.NotEmpty(await dbA.ClassLevels.ToListAsync());
    }

    [Fact]
    public async Task Unknown_tenant_sees_nothing()
    {
        await using var db = fx.CreateDb(new FixedTenantContext(null));
        Assert.Empty(await db.ClassLevels.ToListAsync());
        Assert.Empty(await db.Schools.ToListAsync());
    }

    [Fact]
    public async Task Writing_a_row_for_another_school_is_rejected()
    {
        await using var dbA = fx.CreateDb(new FixedTenantContext(_schoolA));
        dbA.Teachers.Add(new Teacher { SchoolId = _schoolB, Code = "HACK", FirstName = "แอบ", LastName = "ใส่" });
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => dbA.SaveChangesAsync());
    }

    [Fact]
    public async Task New_rows_get_current_school_id_automatically()
    {
        await using var dbB = fx.CreateDb(new FixedTenantContext(_schoolB));
        var room = new Room { Code = "R" + Guid.NewGuid().ToString("N")[..5], Name = "ห้องใหม่" };
        dbB.Rooms.Add(room);
        await dbB.SaveChangesAsync();
        Assert.Equal(_schoolB, room.SchoolId);
    }
}
