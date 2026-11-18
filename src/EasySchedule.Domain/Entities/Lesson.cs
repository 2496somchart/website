using EasySchedule.Domain.Enums;

namespace EasySchedule.Domain.Entities;

/// <summary>คาบสอนที่ต้องจัด: วิชา × ห้องเรียน × ครู, จำนวนคาบต่อสัปดาห์</summary>
public class Lesson : TenantEntity
{
    public int TermId { get; set; }
    public Term? Term { get; set; }
    public int SubjectId { get; set; }
    public Subject? Subject { get; set; }
    public int ClassSectionId { get; set; }
    public ClassSection? ClassSection { get; set; }
    public int? LessonGroupId { get; set; }
    public LessonGroup? LessonGroup { get; set; }
    public int? RoomId { get; set; }
    public Room? Room { get; set; }
    public int PeriodsPerWeek { get; set; }
    /// <summary>จำนวนคาบติดกันต่อครั้ง (1 = ไม่ต้องติดกัน)</summary>
    public int BlockSize { get; set; } = 1;
    public LoadType LoadType { get; set; } = LoadType.Teaching;
    public List<LessonTeacher> Teachers { get; set; } = [];
}
