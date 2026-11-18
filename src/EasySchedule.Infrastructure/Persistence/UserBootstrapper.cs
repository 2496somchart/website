using EasySchedule.Domain.Entities;
using EasySchedule.Domain.Enums;
using EasySchedule.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace EasySchedule.Infrastructure.Persistence;

public static class UserBootstrapper
{
    /// <summary>สร้าง SuperAdmin คนแรกเมื่อระบบยังไม่มีผู้ใช้เลย — คืนค่า true ถ้าสร้าง</summary>
    public static async Task<bool> EnsureFirstSuperAdminAsync(
        EasyScheduleDbContext db, IPasswordService passwords, string username, string? password, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(password)) return false;
        if (!db.BypassTenantFilter)
            throw new InvalidOperationException("Bootstrap must run with SystemTenantContext.");
        if (await db.Users.AnyAsync(ct)) return false;

        var user = new User { Username = username.Trim(), DisplayName = "ผู้ดูแลระบบกลาง", Role = UserRole.SuperAdmin, PasswordHash = "" };
        user.PasswordHash = passwords.Hash(user, password);
        db.Users.Add(user);
        await db.SaveChangesAsync(ct);
        return true;
    }
}
