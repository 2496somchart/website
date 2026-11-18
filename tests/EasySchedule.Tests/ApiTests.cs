using System.Net;
using System.Net.Http.Json;
using EasySchedule.Api.Endpoints;

namespace EasySchedule.Tests;

[Collection(ApiCollection.Name)]
public sealed class ApiTests(ApiFixture fx)
{
    [Fact]
    public async Task Health_returns_ok_with_database()
    {
        var res = await fx.Factory.CreateClient().GetAsync("/api/health");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var body = await res.Content.ReadFromJsonAsync<HealthResponse>();
        Assert.Equal("ok", body!.Status);
        Assert.Equal("ok", body.Database);
    }

    [Fact]
    public async Task Login_with_wrong_password_is_401()
    {
        var res = await fx.Factory.CreateClient().PostAsJsonAsync("/api/auth/login", new LoginRequest("admin", "wrong"));
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task ClassLevels_requires_authentication()
    {
        var schoolId = await fx.SampleSchoolIdAsync();
        var res = await fx.Factory.CreateClient().GetAsync($"/api/schools/{schoolId}/classlevels");
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
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

        var names = levels!.Select(l => l.Name).ToList();
        Assert.Contains("ป.1", names);
        Assert.Contains("ม.1", names);
        Assert.Contains("ม.4", names);
        Assert.All(levels!.Where(l => l.Code is "P1" or "M1" or "M4"), l => Assert.Equal(2, l.Sections.Count));
        Assert.Contains(levels!.SelectMany(l => l.Sections), s => s.Name == "ม.4/2");
    }

    [Fact]
    public async Task Viewer_cannot_create_classlevel()
    {
        var schoolId = await fx.SampleSchoolIdAsync();
        var client = await fx.LoginAsync("viewer");
        var res = await client.PostAsJsonAsync($"/api/schools/{schoolId}/classlevels", new CreateClassLevelRequest("VX", "ทดสอบ", 99, null));
        Assert.Equal(HttpStatusCode.Forbidden, res.StatusCode);
    }

    [Fact]
    public async Task Me_returns_current_user()
    {
        var client = await fx.LoginAsync("admin");
        var me = await client.GetFromJsonAsync<UserDto>("/api/auth/me");
        Assert.Equal("admin", me!.Username);
        Assert.Equal("SchoolAdmin", me.Role);
        Assert.Equal("ผู้ดูแลโรงเรียนตัวอย่าง", me.DisplayName);
    }
}
