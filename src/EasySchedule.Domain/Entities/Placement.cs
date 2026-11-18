namespace EasySchedule.Domain.Entities;

/// <summary>การวาง Lesson ลงช่อง วัน × คาบ (= TblZPlan) ผูกกับ Snapshot</summary>
public class Placement : TermEntity
{
    public int SnapshotId { get; set; }
    public Snapshot? Snapshot { get; set; }
    public int LessonId { get; set; }
    public Lesson? Lesson { get; set; }
    public int DayNo { get; set; }
    public int PeriodNo { get; set; }
    public int? RoomId { get; set; }
    public Room? Room { get; set; }
    public bool IsLocked { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
