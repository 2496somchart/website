namespace EasySchedule.Domain.Entities;

/// <summary>ฉบับของตารางสอน (เก็บผลการจัดแต่ละครั้ง)</summary>
public class Snapshot : TenantEntity
{
    public int TermId { get; set; }
    public Term? Term { get; set; }
    public required string Name { get; set; }
    public bool IsPublished { get; set; }
    public int? CreatedByUserId { get; set; }
    public User? CreatedByUser { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<Placement> Placements { get; set; } = [];
}
