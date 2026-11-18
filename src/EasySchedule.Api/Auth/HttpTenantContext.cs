using System.Security.Claims;
using EasySchedule.Infrastructure.Tenancy;

namespace EasySchedule.Api.Auth;

/// <summary>อ่านโรงเรียนจาก JWT ของคำขอปัจจุบัน; ไม่มี claim = ไม่เห็นข้อมูลใด</summary>
public sealed class HttpTenantContext(IHttpContextAccessor accessor) : ITenantContext
{
    private ClaimsPrincipal? User => accessor.HttpContext?.User;

    public int? SchoolId =>
        int.TryParse(User?.FindFirstValue(AppClaims.SchoolId), out var id) ? id : null;

    public bool BypassFilter => User?.IsInRole(Roles.SuperAdmin) == true;
}

public static class ClaimsPrincipalExtensions
{
    /// <summary>ผู้ใช้เข้าถึงข้อมูลของโรงเรียนนี้ได้หรือไม่</summary>
    public static bool CanAccessSchool(this ClaimsPrincipal user, int schoolId) =>
        user.IsInRole(Roles.SuperAdmin)
        || (int.TryParse(user.FindFirstValue(AppClaims.SchoolId), out var sid) && sid == schoolId);
}
