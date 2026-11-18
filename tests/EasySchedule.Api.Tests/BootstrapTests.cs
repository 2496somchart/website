using EasySchedule.Domain.Enums;
using EasySchedule.Infrastructure.Persistence;
using EasySchedule.Infrastructure.Security;
using EasySchedule.Infrastructure.Tenancy;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace EasySchedule.Api.Tests;

/// <summary>ติดตั้งครั้งแรก: สร้าง SuperAdmin จาก env เฉพาะเมื่อยังไม่มีผู้ใช้</summary>
[Collection(ApiCollection.Name)]
public sealed class BootstrapTests(ApiFixture fx)
{
    [Fact]
    public async Task Creates_first_superadmin_only_on_empty_database()
    {
        var cs = new SqlConnectionStringBuilder(fx.ConnectionString) { InitialCatalog = "EasyScheduleWeb_Boot_" + Guid.NewGuid().ToString("N")[..8] }.ConnectionString;
        var options = new DbContextOptionsBuilder<EasyScheduleDbContext>().UseSqlServer(cs).Options;
        await using var db = new EasyScheduleDbContext(options, SystemTenantContext.Instance);
        await db.Database.MigrateAsync();
        try
        {
            var passwords = new PasswordService();

            (await UserBootstrapper.EnsureFirstSuperAdminAsync(db, passwords, "root", null)).Should().BeFalse("ไม่มีรหัสผ่าน = ไม่สร้าง");
            (await UserBootstrapper.EnsureFirstSuperAdminAsync(db, passwords, "root", "Boot-Aa1!" + Guid.NewGuid())).Should().BeTrue();
            (await UserBootstrapper.EnsureFirstSuperAdminAsync(db, passwords, "root2", "Other-Aa1!")).Should().BeFalse("มีผู้ใช้แล้ว");

            var users = await db.Users.ToListAsync();
            users.Should().ContainSingle().Which.Should().Match<Domain.Entities.User>(u =>
                u.Username == "root" && u.Role == UserRole.SuperAdmin && u.SchoolId == null);
            (await db.Schools.AnyAsync()).Should().BeFalse("bootstrap ไม่สร้างข้อมูลตัวอย่าง");
        }
        finally
        {
            await db.Database.EnsureDeletedAsync();
        }
    }
}
