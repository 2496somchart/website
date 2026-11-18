using EasySchedule.Domain.Enums;

namespace EasySchedule.Domain.Entities;

/// <summary>กฎระหว่างสองวิชา (= TblXPlanConstraint): C ต่อเนื่อง / U ห้ามวันเดียวกัน / S ลำดับ</summary>
public class SubjectRule : TermEntity
{
    public int Subject1Id { get; set; }
    public Subject? Subject1 { get; set; }
    public int Subject2Id { get; set; }
    public Subject? Subject2 { get; set; }
    public SubjectRuleType Type { get; set; }
}
