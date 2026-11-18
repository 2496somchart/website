namespace EasySchedule.Domain.Entities;

/// <summary>แผนการเรียน: ระดับชั้นนี้เรียนวิชานี้กี่คาบ/สัปดาห์ในภาคเรียนนี้</summary>
public class StudyPlan : TenantEntity
{
    public int TermId { get; set; }
    public Term? Term { get; set; }
    public int ClassLevelId { get; set; }
    public ClassLevel? ClassLevel { get; set; }
    public int SubjectId { get; set; }
    public Subject? Subject { get; set; }
    public int PeriodsPerWeek { get; set; }
}
