using EasySchedule.Domain.Enums;

namespace EasySchedule.Domain.Entities;

/// <summary>โรงเรียน (= TblSchoolInfo) พร้อมข้อมูล license (SPEC ข้อ 8)</summary>
public class School
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string SchCode { get; set; }

    public LicensePlan Plan { get; set; } = LicensePlan.Trial;

    /// <summary>License tier: จำนวนห้องต่อระดับชั้นสูงสุด 4/8/12/16/20 (Trial = 4)</summary>
    public int MaxSectionsPerLevel { get; set; } = 4;

    /// <summary>หมดอายุแล้วดูตารางเดิมได้ แต่จัดใหม่ไม่ได้</summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>เพดานจำนวนคาบที่วางได้ตอนทดลอง</summary>
    public int TrialPlacementCap { get; set; } = 200;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public static readonly int[] AllowedTiers = [4, 8, 12, 16, 20];
}
