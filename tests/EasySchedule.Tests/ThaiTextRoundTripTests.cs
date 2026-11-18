using System.Net;
using System.Net.Http.Json;
using System.Text;
using EasySchedule.Api.Endpoints;
using EasySchedule.Infrastructure.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace EasySchedule.Tests;

[Collection(ApiCollection.Name)]
public sealed class ThaiTextRoundTripTests(ApiFixture fx)
{
    // ครอบคลุมสระบน/ล่าง วรรณยุกต์ซ้อน ตัวเลขไทย ไม้ยมก ไปยาลน้อย และ emoji
    public const string ThaiSample = "ม.6/๑ ห้องเรียนพิเศษ “วิทย์-คณิต” ก็ได้ น้ำใจ ฤๅษี ปีใหม่ ๆ ฯลฯ ๐๑๒๓๔๕๖๗๘๙ เก๋ไก๊ 🏫";

    [Fact]
    public async Task Thai_text_round_trips_through_api_and_database_unchanged()
    {
        var schoolId = await fx.SampleSchoolIdAsync();
        var client = await fx.LoginAsync("admin");
        var code = "TH" + Guid.NewGuid().ToString("N")[..6];

        var create = await client.PostAsJsonAsync($"/api/schools/{schoolId}/classlevels",
            new CreateClassLevelRequest(code, ThaiSample, 50, null));
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var created = await create.Content.ReadFromJsonAsync<ClassLevelDto>();
        Assert.Equal(ThaiSample, created!.Name);

        // อ่านกลับผ่าน API — ต้องได้ข้อความเดิมทุก byte
        var levels = await client.GetFromJsonAsync<List<ClassLevelDto>>($"/api/schools/{schoolId}/classlevels");
        var fetched = Assert.Single(levels!, l => l.Code == code);
        Assert.Equal(ThaiSample, fetched.Name);
        Assert.Equal(Encoding.UTF8.GetBytes(ThaiSample), Encoding.UTF8.GetBytes(fetched.Name));

        // response เป็น UTF-8 จริง
        var raw = await client.GetAsync($"/api/schools/{schoolId}/classlevels");
        Assert.Equal("utf-8", raw.Content.Headers.ContentType?.CharSet, ignoreCase: true);

        // ในฐานข้อมูลเก็บเป็น nvarchar ไม่กลายเป็น '?'
        await using var db = fx.CreateDb(SystemTenantContext.Instance);
        var stored = await db.ClassLevels.Where(l => l.Code == code).Select(l => l.Name).SingleAsync();
        Assert.Equal(ThaiSample, stored);
        Assert.DoesNotContain('?', stored);
    }

    [Fact]
    public async Task Thai_sorting_uses_thai_collation()
    {
        await using var db = fx.CreateDb(SystemTenantContext.Instance);
        var collation = await db.Database.SqlQueryRaw<string>(
            "SELECT CAST(DATABASEPROPERTYEX(DB_NAME(), 'Collation') AS nvarchar(128)) AS [Value]").SingleAsync();
        Assert.Equal("Thai_100_CI_AS", collation);
    }
}
