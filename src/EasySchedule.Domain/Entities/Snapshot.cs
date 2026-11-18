namespace EasySchedule.Domain.Entities;

/// <summary>ผลการจัดตารางแต่ละครั้ง (แทน TblZPlanPark). โหมดปัจจุบันตัดสินจาก Snapshot ที่ IsLive</summary>
public class Snapshot : TermEntity
{
    public int ModeId { get; set; }
    public ScheduleMode? Mode { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int? CreatedBy { get; set; }
    public User? CreatedByUser { get; set; }
    public string? Note { get; set; }
    public bool IsLive { get; set; }
    public List<Placement> Placements { get; set; } = [];
}
