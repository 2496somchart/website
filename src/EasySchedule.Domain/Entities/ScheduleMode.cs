using EasySchedule.Domain.Enums;

namespace EasySchedule.Domain.Entities;

/// <summary>รูปแบบตาราง (= TblSchedulePlan) ใช้พร้อมกันทั้งโรงเรียน</summary>
public class ScheduleMode : TermEntity
{
    public ScheduleModeType Mode { get; set; }

    /// <summary>DayNo ที่มีเรียน (2-6) — เก็บเป็น JSON array ไม่ใช่ string คั่น comma</summary>
    public List<int> DaysList { get; set; } = [2, 3, 4, 5, 6];

    public int PeriodPerDay { get; set; } = 8;

    /// <summary>วัน ร.ด. (DayNo) — เฉพาะโหมด 1, ห้าม hardcode</summary>
    public int? MilitaryDay { get; set; }

    public List<Period> Periods { get; set; } = [];
}
