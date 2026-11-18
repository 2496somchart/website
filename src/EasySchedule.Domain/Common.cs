using EasySchedule.Domain.Entities;

namespace EasySchedule.Domain;

/// <summary>Entity ที่ผูกกับโรงเรียน — ถูกกรองด้วย global query filter ตาม SchoolId เสมอ</summary>
public interface ITenantEntity
{
    int SchoolId { get; set; }
}

public abstract class TenantEntity : ITenantEntity
{
    public int Id { get; set; }
    public int SchoolId { get; set; }
    public School? School { get; set; }
}

/// <summary>Entity ที่ผูกกับปี/เทอมของงานจัดตาราง (SPEC ข้อ 3: ทุกตารางมี SchoolId + TermId)</summary>
public abstract class TermEntity : TenantEntity
{
    public int TermId { get; set; }
    public Term? Term { get; set; }
}

/// <summary>ค่าคงที่ของปฏิทินตาราง (ตามระบบเดิม)</summary>
public static class Calendar
{
    /// <summary>DayNo 2-6 = จันทร์-ศุกร์</summary>
    public const int FirstDayNo = 2;
    public const int LastDayNo = 6;

    /// <summary>PeriodNo 0 = โฮมรูม (ไม่นับเป็นคาบสอน)</summary>
    public const int HomeroomPeriodNo = 0;
    public const int MaxPeriodNo = 12;

    public static string DayNameThai(int dayNo) => dayNo switch
    {
        1 => "อาทิตย์",
        2 => "จันทร์",
        3 => "อังคาร",
        4 => "พุธ",
        5 => "พฤหัสบดี",
        6 => "ศุกร์",
        7 => "เสาร์",
        _ => throw new ArgumentOutOfRangeException(nameof(dayNo), dayNo, null),
    };
}
