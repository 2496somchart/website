namespace EasySchedule.Domain.Entities;

/// <summary>แผนการเรียน (= PlanTitleList) — ตารางลูก ไม่ใช่ string คั่น comma</summary>
public class StudyPlan : TermEntity
{
    public required string Name { get; set; }
    public List<StudyPlanSection> Sections { get; set; } = [];
}

/// <summary>ห้องเรียนที่อยู่ในแผนการเรียน (ห้องเดียวมีหลายแผนได้)</summary>
public class StudyPlanSection : ITenantEntity
{
    public int SchoolId { get; set; }
    public int StudyPlanId { get; set; }
    public StudyPlan? StudyPlan { get; set; }
    public int SectionId { get; set; }
    public ClassSection? Section { get; set; }
}
