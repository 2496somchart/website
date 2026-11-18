namespace EasySchedule.Domain.Entities;

/// <summary>คาบเวลา (= TblPeriod) PeriodNo 0-12, P0 = โฮมรูม</summary>
public class Period : TermEntity
{
    public int ScheduleModeId { get; set; }
    public ScheduleMode? ScheduleMode { get; set; }
    public int PeriodNo { get; set; }
    public TimeOnly Start { get; set; }
    public TimeOnly End { get; set; }
}
