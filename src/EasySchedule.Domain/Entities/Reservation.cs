using EasySchedule.Domain.Enums;

namespace EasySchedule.Domain.Entities;

/// <summary>
/// บล็อกเวลา (= TblEPlan + LoadType 3/4) รายแผนการเรียน / รายครู / รายห้อง —
/// ระบุเป้าหมายอย่างน้อย 1 อย่าง
/// </summary>
public class Reservation : TermEntity
{
    public int DayNo { get; set; }
    public int PeriodNo { get; set; }
    public LoadType LoadType { get; set; } = LoadType.Personal;

    public int? StudyPlanId { get; set; }
    public StudyPlan? StudyPlan { get; set; }
    public int? TeacherId { get; set; }
    public Teacher? Teacher { get; set; }
    public int? RoomId { get; set; }
    public Room? Room { get; set; }

    public string? Note { get; set; }
}
