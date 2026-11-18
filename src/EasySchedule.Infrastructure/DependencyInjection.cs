using EasySchedule.Infrastructure.Persistence;
using EasySchedule.Infrastructure.Security;
using EasySchedule.Infrastructure.Tenancy;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EasySchedule.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var cs = ResolveConnectionString(config);

        services.AddDbContext<EasyScheduleDbContext>(o => o.UseSqlServer(cs, sql => sql.EnableRetryOnFailure()));
        services.AddSingleton<IPasswordService, PasswordService>();
        services.Configure<SeedOptions>(config.GetSection(SeedOptions.Section));
        services.Configure<BootstrapOptions>(config.GetSection(BootstrapOptions.Section));
        return services;
    }

    /// <summary>
    /// ใช้ ConnectionStrings:Default ถ้ามี; ไม่งั้นประกอบจาก Database:Server/Name/User/Password
    /// (docker compose ใช้แบบหลัง เพื่อให้รหัสผ่านอยู่ใน .env เท่านั้น)
    /// </summary>
    public static string ResolveConnectionString(IConfiguration config)
    {
        var cs = config.GetConnectionString("Default");
        if (!string.IsNullOrWhiteSpace(cs)) return cs;

        var db = config.GetSection("Database");
        var server = db["Server"];
        if (string.IsNullOrWhiteSpace(server))
            throw new InvalidOperationException("Database is not configured: set ConnectionStrings__Default or Database__Server (+ Database__User, Database__Password).");

        var b = new SqlConnectionStringBuilder
        {
            DataSource = server,
            InitialCatalog = db["Name"] ?? "EasyScheduleWeb",
            TrustServerCertificate = db.GetValue("TrustServerCertificate", true),
            Encrypt = db.GetValue("Encrypt", true),
        };
        if (string.IsNullOrWhiteSpace(db["User"]))
        {
            b.IntegratedSecurity = true;
        }
        else
        {
            b.UserID = db["User"];
            b.Password = db["Password"] ?? throw new InvalidOperationException("Database__Password is required when Database__User is set.");
        }
        return b.ConnectionString;
    }

    /// <summary>รัน migration และ seed (ถ้าเปิดไว้) ด้วยสิทธิ์ระบบ</summary>
    public static async Task InitializeDatabaseAsync(this IServiceProvider root, bool migrate, CancellationToken ct = default)
    {
        using var scope = root.CreateScope();
        var sp = scope.ServiceProvider;
        var options = sp.GetRequiredService<DbContextOptions<EasyScheduleDbContext>>();
        await using var db = new EasyScheduleDbContext(options, SystemTenantContext.Instance);

        if (migrate)
            await db.Database.MigrateAsync(ct);

        var bootstrap = sp.GetRequiredService<IOptions<BootstrapOptions>>().Value;
        if (await UserBootstrapper.EnsureFirstSuperAdminAsync(db, sp.GetRequiredService<IPasswordService>(),
                bootstrap.SuperAdminUsername, bootstrap.SuperAdminPassword, ct))
        {
            sp.GetRequiredService<ILogger<DataSeeder>>().LogInformation("Created first SuperAdmin '{User}'", bootstrap.SuperAdminUsername);
        }

        var seed = sp.GetRequiredService<IOptions<SeedOptions>>().Value;
        if (seed.Enabled)
        {
            var seeder = new DataSeeder(db, sp.GetRequiredService<IPasswordService>(), sp.GetRequiredService<ILogger<DataSeeder>>());
            await seeder.SeedAsync(seed.UserPassword, ct);
        }
    }
}
