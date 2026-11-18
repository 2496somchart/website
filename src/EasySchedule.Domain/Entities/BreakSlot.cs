namespace EasySchedule.Domain.Entities;

/// <summary>
/// คาบพักรายห้อง-รายวัน (SectionId × DayNo × PeriodNo) แยกตามโหมด.
/// ค่าเริ่มต้นตั้งให้ตรงกันทั้งโรงเรียน แล้วแก้ได้อิสระทุกระดับ ทุกห้อง ทุกวัน
/// </summary>
public class BreakSlot : TermEntity
{
    public int ScheduleModeId { get; set; }
    public ScheduleMode? ScheduleMode { get; set; }
    public int SectionId { get; set; }
    public ClassSection? Section { get; set; }
    public int DayNo { get; set; }
    public int PeriodNo { get; set; }
}
