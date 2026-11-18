namespace EasySchedule.Infrastructure.Persistence;

public sealed class SeedOptions
{
    public const string Section = "Seed";

    /// <summary>seed ข้อมูลตัวอย่างตอนเริ่มระบบ (ปิดใน production จริง)</summary>
    public bool Enabled { get; set; }

    /// <summary>รหัสผ่านของ user ตัวอย่าง — ต้องมาจาก env/secret เท่านั้น ห้าม commit</summary>
    public string? UserPassword { get; set; }
}

/// <summary>
/// ติดตั้งครั้งแรก (on-prem/cloud): ถ้ายังไม่มีผู้ใช้เลย สร้าง SuperAdmin 1 คนจาก env แล้วไม่ทำอะไรอีก.
/// ไม่สร้างข้อมูลตัวอย่าง
/// </summary>
public sealed class BootstrapOptions
{
    public const string Section = "Bootstrap";
    public string SuperAdminUsername { get; set; } = "superadmin";
    public string? SuperAdminPassword { get; set; }
}
