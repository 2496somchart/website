using EasySchedule.Domain.Enums;

namespace EasySchedule.Domain.Entities;

/// <summary>ภาระสอน 1 วิชา × 1 ห้อง (= TblXPlan)</summary>
public class Lesson : TermEntity
{
    public int SectionId { get; set; }
    public ClassSection? Section { get; set; }
    public int SubjectId { get; set; }
    public Subject? Subject { get; set; }
    public LoadType LoadType { get; set; } = LoadType.Teaching;

    /// <summary>คาบต่อสัปดาห์</summary>
    public int PPW { get; set; }

    /// <summary>คาบต่อครั้ง</summary>
    public int PT { get; set; } = 1;

    public int? RoomId { get; set; }
    public Room? Room { get; set; }
    public int? LabId { get; set; }
    public Room? Lab { get; set; }

    /// <summary>วิชาเดียวกันในวันเดียวกันแยกช่วงได้ (ยกเว้น H5/H11)</summary>
    public bool AllowSplit { get; set; }

    /// <summary>ติดกันได้ไม่เกิน (null = ใช้ค่าตามกฎทั่วไป)</summary>
    public int? MaxConsec { get; set; }

    public int? LessonGroupId { get; set; }
    public LessonGroup? LessonGroup { get; set; }

    /// <summary>แผนการเรียนของวิชาเลือก (LoadType 6)</summary>
    public int? StudyPlanId { get; set; }
    public StudyPlan? StudyPlan { get; set; }

    public List<LessonTeacher> Teachers { get; set; } = [];
}
