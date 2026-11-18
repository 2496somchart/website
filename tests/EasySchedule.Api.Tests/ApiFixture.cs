using System.Net.Http.Headers;
using System.Net.Http.Json;
using EasySchedule.Api.Endpoints;
using EasySchedule.Infrastructure.Persistence;
using EasySchedule.Infrastructure.Tenancy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;

namespace EasySchedule.Api.Tests;

/// <summary>
/// SQL Server จริงใน Docker (Testcontainers) + API ผ่าน WebApplicationFactory.
/// ตั้ง env ES_TEST_SQLSERVER (connection string ไปยัง server ที่มีอยู่) เพื่อข้าม Docker ได้
/// </summary>
public sealed class ApiFixture : IAsyncLifetime
{
    // ค่าทดสอบสร้างแบบสุ่มทุกครั้ง ไม่มีรหัสผ่านตายตัวใน repo
    public static readonly string SeedPassword = "T3st!" + Guid.NewGuid().ToString("N");
    private static readonly string JwtKey = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");

    private MsSqlContainer? _container;
    public string ConnectionString { get; private set; } = "";
    public WebApplicationFactory<Program> Factory { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var external = Environment.GetEnvironmentVariable("ES_TEST_SQLSERVER");
        string serverCs;
        if (!string.IsNullOrWhiteSpace(external))
        {
            serverCs = external;
        }
        else
        {
            _container = new MsSqlBuilder().WithImage("mcr.microsoft.com/mssql/server:2022-latest").Build();
            await _container.StartAsync();
            serverCs = _container.GetConnectionString();
        }

        ConnectionString = new SqlConnectionStringBuilder(serverCs)
        {
            InitialCatalog = "EasyScheduleWeb_Test_" + Guid.NewGuid().ToString("N")[..8],
            TrustServerCertificate = true,
        }.ConnectionString;

        Factory = new WebApplicationFactory<Program>().WithWebHostBuilder(b =>
        {
            b.UseEnvironment("Testing");
            b.UseSetting("ConnectionStrings:Default", ConnectionString);
            b.UseSetting("Jwt:SigningKey", JwtKey);
            b.UseSetting("Database:MigrateOnStartup", "true");
            b.UseSetting("Seed:Enabled", "true");
            b.UseSetting("Seed:UserPassword", SeedPassword);
        });
        _ = Factory.Server; // start host → migrate + seed
    }

    public async Task DisposeAsync()
    {
        try
        {
            await using var db = CreateDb(SystemTenantContext.Instance);
            await db.Database.EnsureDeletedAsync();
        }
        catch { /* best effort */ }
        await Factory.DisposeAsync();
        if (_container is not null) await _container.DisposeAsync();
    }

    public EasyScheduleDbContext CreateDb(ITenantContext tenant) =>
        new(new DbContextOptionsBuilder<EasyScheduleDbContext>().UseSqlServer(ConnectionString).Options, tenant);

    public async Task<HttpClient> LoginAsync(string username, string? password = null)
    {
        var client = Factory.CreateClient();
        var res = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(username, password ?? SeedPassword));
        res.EnsureSuccessStatusCode();
        var body = (await res.Content.ReadFromJsonAsync<LoginResponse>())!;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", body.Token);
        return client;
    }

    public async Task<int> ActiveTermIdAsync(int schoolId)
    {
        await using var db = CreateDb(SystemTenantContext.Instance);
        return await db.Terms.Where(t => t.SchoolId == schoolId && t.IsActive).Select(t => t.Id).SingleAsync();
    }

    public async Task<int> SampleSchoolIdAsync()
    {
        await using var db = CreateDb(SystemTenantContext.Instance);
        return await db.Schools.Where(s => s.SchCode == DataSeeder.SampleSchCode).Select(s => s.Id).SingleAsync();
    }
}

[CollectionDefinition(Name)]
public sealed class ApiCollection : ICollectionFixture<ApiFixture>
{
    public const string Name = "api";
}
