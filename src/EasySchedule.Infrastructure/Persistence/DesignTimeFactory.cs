using EasySchedule.Infrastructure.Tenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EasySchedule.Infrastructure.Persistence;

/// <summary>ใช้กับ `dotnet ef migrations add` เท่านั้น (ไม่ต่อฐานข้อมูลจริง)</summary>
public sealed class DesignTimeFactory : IDesignTimeDbContextFactory<EasyScheduleDbContext>
{
    public EasyScheduleDbContext CreateDbContext(string[] args)
    {
        var cs = Environment.GetEnvironmentVariable("ConnectionStrings__Default")
                 ?? "Server=localhost;Database=EasyScheduleWeb;Integrated Security=true;TrustServerCertificate=true";
        var options = new DbContextOptionsBuilder<EasyScheduleDbContext>().UseSqlServer(cs).Options;
        return new EasyScheduleDbContext(options, SystemTenantContext.Instance);
    }
}
