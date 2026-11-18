namespace EasySchedule.Domain.Entities;

/// <summary>
/// ช่วงพัก (พักกลางวัน/พักเบรก) ต่อจากคาบที่ AfterPeriod.
/// ClassLevelId = null หมายถึงค่าเริ่มต้นของทั้งโรงเรียน; Day = null หมายถึงทุกวัน
/// </summary>
public class BreakSlot : TenantEntity
{
    public required string Name { get; set; }
    public int AfterPeriod { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public Enums.SchoolDay? Day { get; set; }
    public int? ClassLevelId { get; set; }
    public ClassLevel? ClassLevel { get; set; }
}
