namespace EasySchedule.Domain.Entities;

/// <summary>รูปแบบตารางเวลา (จำนวนวัน/คาบต่อวัน) เช่น "ประถม" กับ "มัธยม" อาจใช้คาบต่างกัน</summary>
public class ScheduleMode : TenantEntity
{
    public required string Name { get; set; }
    public int DaysPerWeek { get; set; } = 5;
    public int PeriodsPerDay { get; set; } = 8;
    public List<Period> Periods { get; set; } = [];
}
