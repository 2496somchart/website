using EasySchedule.Domain.Enums;

namespace EasySchedule.Domain.Entities;

/// <summary>ผู้ใช้ระบบ. SchoolId = null สำหรับ SuperAdmin เท่านั้น</summary>
public class User
{
    public int Id { get; set; }
    public int? SchoolId { get; set; }
    public School? School { get; set; }
    public required string Username { get; set; }
    public required string PasswordHash { get; set; }
    public required string DisplayName { get; set; }
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
