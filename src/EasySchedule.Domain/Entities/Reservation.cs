using EasySchedule.Domain.Enums;

namespace EasySchedule.Domain.Entities;

/// <summary>การจองช่องเวลาล่วงหน้า (ครู/ห้อง/ห้องเรียน ไม่ว่างในวัน-คาบนี้)</summary>
public class Reservation : TenantEntity
{
    public int TermId { get; set; }
    public Term? Term { get; set; }
    public SchoolDay Day { get; set; }
    public int PeriodNumber { get; set; }
    public int? TeacherId { get; set; }
    public Teacher? Teacher { get; set; }
    public int? RoomId { get; set; }
    public Room? Room { get; set; }
    public int? ClassSectionId { get; set; }
    public ClassSection? ClassSection { get; set; }
    public string? Reason { get; set; }
}
