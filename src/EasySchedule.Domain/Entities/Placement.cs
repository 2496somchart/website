using EasySchedule.Domain.Enums;

namespace EasySchedule.Domain.Entities;

/// <summary>การวาง Lesson ลงช่องวัน-คาบ ภายใน Snapshot หนึ่ง</summary>
public class Placement : TenantEntity
{
    public int SnapshotId { get; set; }
    public Snapshot? Snapshot { get; set; }
    public int LessonId { get; set; }
    public Lesson? Lesson { get; set; }
    public SchoolDay Day { get; set; }
    public int PeriodNumber { get; set; }
    public int? RoomId { get; set; }
    public Room? Room { get; set; }
    public bool IsLocked { get; set; }
}
