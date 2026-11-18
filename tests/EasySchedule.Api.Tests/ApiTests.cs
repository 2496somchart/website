using System.Net;
using System.Net.Http.Json;
using EasySchedule.Api.Endpoints;
using FluentAssertions;

namespace EasySchedule.Api.Tests;

[Collection(ApiCollection.Name)]
public sealed class ApiTests(ApiFixture fx)
{
    [Fact]
    public async Task Health_returns_ok_with_database()
    {
        var res = await fx.Factory.CreateClient().GetAsync("/api/health");
        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await res.Content.ReadFromJsonAsync<HealthResponse>();
        body!.Status.Should().Be("ok");
        body.Database.Should().Be("ok");
    }

    [Fact]
    public async Task Login_with_wrong_password_is_401_with_thai_message()
    {
        var res = await fx.Factory.CreateClient().PostAsJsonAsync("/api/auth/login", new LoginRequest("admin", "wrong"));
        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await res.Content.ReadAsStringAsync()).Should().Contain("ชื่อผู้ใช้หรือรหัสผ่านไม่ถูกต้อง");
    }

    [Fact]
    public async Task ClassLevels_requires_authentication()
    {
        var schoolId = await fx.SampleSchoolIdAsync();
        var res = await fx.Factory.CreateClient().GetAsync($"/api/schools/{schoolId}/classlevels");
        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("admin")]
    [InlineData("viewer")]
    [InlineData("superadmin")]
    public async Task Seeded_school_classlevels_are_returned_in_thai(string username)
    {
        var schoolId = await fx.SampleSchoolIdAsync();
        var client = await fx.LoginAsync(username);

        var levels = await client.GetFromJsonAsync<List<ClassLevelDto>>($"/api/schools/{schoolId}/classlevels");

        levels!.Where(l => l.Code is 11 or 21 or 34).Select(l => (l.Code, l.Name))
            .Should().Equal((11, "ป.1"), (21, "ม.1"), (34, "ม.4"));
        levels!.Where(l => l.Code is 11 or 21 or 34).Should().OnlyContain(l => l.Sections.Count == 2);
        levels!.SelectMany(l => l.Sections).Select(s => s.Name).Should().Contain("ม.4/2");
    }

    [Fact]
    public async Task Viewer_cannot_create_classlevel()
    {
        var schoolId = await fx.SampleSchoolIdAsync();
        var client = await fx.LoginAsync("viewer");
        var res = await client.PostAsJsonAsync($"/api/schools/{schoolId}/classlevels", new CreateClassLevelRequest(12, "ป.2", 1, 8, null));
        res.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Invalid_classlevel_code_is_rejected_with_thai_reason()
    {
        var schoolId = await fx.SampleSchoolIdAsync();
        var client = await fx.LoginAsync("admin");
        var res = await client.PostAsJsonAsync($"/api/schools/{schoolId}/classlevels", new CreateClassLevelRequest(5, "ผิด", 1, 8, null));
        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await res.Content.ReadAsStringAsync()).Should().Contain("รหัสระดับชั้นต้องอยู่ระหว่าง 10-99");
    }

    [Fact]
    public async Task Me_returns_current_user()
    {
        var client = await fx.LoginAsync("admin");
        var me = await client.GetFromJsonAsync<UserDto>("/api/auth/me");
        me!.Username.Should().Be("admin");
        me.Role.Should().Be("SchoolAdmin");
        me.DisplayName.Should().Be("ฝ่ายวิชาการ โรงเรียนตัวอย่าง");
    }
}
