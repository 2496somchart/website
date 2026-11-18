namespace EasySchedule.Domain.Entities;

/// <summary>คาบเรียนที่ N ของรูปแบบตาราง พร้อมเวลาเริ่ม/จบ</summary>
public class Period : TenantEntity
{
    public int ScheduleModeId { get; set; }
    public ScheduleMode? ScheduleMode { get; set; }
    public int Number { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
}
