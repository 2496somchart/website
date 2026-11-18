namespace EasySchedule.Domain.Entities;

/// <summary>
/// แผนการเรียน (= PlanTitleList) เช่น "วิทย์-คณิต" — <b>ข้อมูลระดับโรงเรียน</b> ใช้ได้หลายปี/เทอม.
/// รายวิชาในแผนกำหนดรายระดับชั้น-รายภาคเรียน; โรงเรียนที่ไม่ทำแผนไม่ต้องมี record นี้
/// </summary>
public class StudyPlan : TenantEntity
{
    public required string Name { get; set; }
    public List<StudyPlanSubject> Subjects { get; set; } = [];
    public List<StudyPlanSection> Sections { get; set; } = [];
}

/// <summary>วิชาในแผน: ระดับชั้น (Code เช่น 34 = ม.4) × ภาคเรียน (1/2) → วิชา</summary>
public class StudyPlanSubject : TenantEntity
{
    public int StudyPlanId { get; set; }
    public StudyPlan? StudyPlan { get; set; }

    /// <summary>รหัสระดับชั้น (11-16, 21-23, 34-36) — ใช้รหัสเพราะแผนครอบคลุมหลายปี ส่วน ClassLevel ผูกเทอม</summary>
    public int LevelCode { get; set; }

    /// <summary>ภาคเรียนที่เรียน 1 หรือ 2</summary>
    public int Semester { get; set; }

    public int SubjectId { get; set; }
    public Subject? Subject { get; set; }

    /// <summary>คาบต่อสัปดาห์ตามแผน (null = ใช้ค่าของ Subject)</summary>
    public int? PPW { get; set; }
}

/// <summary>ห้องเรียน (ของเทอมนั้น) ที่ใช้แผนการเรียนนี้ — ห้องเดียวมีหลายแผนได้</summary>
public class StudyPlanSection : ITenantEntity
{
    public int SchoolId { get; set; }
    public int StudyPlanId { get; set; }
    public StudyPlan? StudyPlan { get; set; }
    public int SectionId { get; set; }
    public ClassSection? Section { get; set; }
}
