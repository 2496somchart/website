using System.Net;
using System.Net.Http.Json;
using System.Text;
using EasySchedule.Api.Endpoints;
using EasySchedule.Infrastructure.Tenancy;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace EasySchedule.Api.Tests;

[Collection(ApiCollection.Name)]
public sealed class ThaiTextRoundTripTests(ApiFixture fx)
{
    // สระบน/ล่าง วรรณยุกต์ซ้อน ตัวเลขไทย ไม้ยมก ไปยาลน้อย ฤๅ และ emoji
    public const string ThaiSample = "ม.6 ห้องเรียนพิเศษ “วิทย์-คณิต” ก็ได้ น้ำใจ ฤๅษี ปีใหม่ ๆ ฯลฯ ๐๑๒๓๔๕๖๗๘๙ เก๋ไก๊ 🏫";

    [Fact]
    public async Task Thai_text_round_trips_through_api_and_database_unchanged()
    {
        var schoolId = await fx.SampleSchoolIdAsync();
        var client = await fx.LoginAsync("admin");

        var create = await client.PostAsJsonAsync($"/api/schools/{schoolId}/classlevels",
            new CreateClassLevelRequest(36, ThaiSample, 1, 10, null));
        create.StatusCode.Should().Be(HttpStatusCode.Created);
        (await create.Content.ReadFromJsonAsync<ClassLevelDto>())!.Name.Should().Be(ThaiSample);

        // อ่านกลับผ่าน API — ต้องได้ข้อความเดิมทุก byte
        var levels = await client.GetFromJsonAsync<List<ClassLevelDto>>($"/api/schools/{schoolId}/classlevels");
        var fetched = levels!.Should().ContainSingle(l => l.Code == 36).Subject;
        Encoding.UTF8.GetBytes(fetched.Name).Should().Equal(Encoding.UTF8.GetBytes(ThaiSample));

        var raw = await client.GetAsync($"/api/schools/{schoolId}/classlevels");
        raw.Content.Headers.ContentType!.CharSet.Should().BeEquivalentTo("utf-8");

        // ในฐานข้อมูลเก็บเป็น nvarchar ไม่กลายเป็น '?'
        await using var db = fx.CreateDb(SystemTenantContext.Instance);
        var stored = await db.ClassLevels.Where(l => l.SchoolId == schoolId && l.Code == 36).Select(l => l.Name).SingleAsync();
        stored.Should().Be(ThaiSample).And.NotContain("?");
    }

    [Fact]
    public async Task Database_uses_thai_collation()
    {
        await using var db = fx.CreateDb(SystemTenantContext.Instance);
        var collation = await db.Database.SqlQueryRaw<string>(
            "SELECT CAST(DATABASEPROPERTYEX(DB_NAME(), 'Collation') AS nvarchar(128)) AS [Value]").SingleAsync();
        collation.Should().Be("Thai_100_CI_AS");
    }

    [Fact]
    public async Task All_text_columns_are_unicode()
    {
        await using var db = fx.CreateDb(SystemTenantContext.Instance);
        var nonUnicode = await db.Database.SqlQueryRaw<string>("""
            SELECT t.name + '.' + c.name AS [Value]
            FROM sys.columns c JOIN sys.tables t ON t.object_id = c.object_id
            JOIN sys.types ty ON ty.user_type_id = c.user_type_id
            WHERE ty.name IN ('varchar', 'char', 'text') AND t.name <> '__EFMigrationsHistory'
            """).ToListAsync();
        nonUnicode.Should().BeEmpty("ทุกคอลัมน์ข้อความต้องเป็น nvarchar (CLAUDE.md กฎข้อ 6)");
    }
}
