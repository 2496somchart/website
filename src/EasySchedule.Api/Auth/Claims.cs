namespace EasySchedule.Api.Auth;

public static class AppClaims
{
    public const string SchoolId = "school_id";
    public const string Role = "role";
}

public static class Roles
{
    public const string SuperAdmin = nameof(Domain.Enums.UserRole.SuperAdmin);
    public const string SchoolAdmin = nameof(Domain.Enums.UserRole.SchoolAdmin);
    public const string Viewer = nameof(Domain.Enums.UserRole.Viewer);
}

public static class Policies
{
    /// <summary>ผู้ใช้ทุก role ที่ login แล้ว</summary>
    public const string AnyUser = nameof(AnyUser);
    /// <summary>แก้ไขข้อมูลได้: SuperAdmin, SchoolAdmin</summary>
    public const string CanEdit = nameof(CanEdit);
    public const string SuperAdminOnly = nameof(SuperAdminOnly);
}
