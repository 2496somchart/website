namespace EasySchedule.Domain.Entities;

public class AuditLog
{
    public long Id { get; set; }
    public int? SchoolId { get; set; }
    public int? UserId { get; set; }
    public required string Action { get; set; }
    public required string EntityName { get; set; }
    public string? EntityId { get; set; }
    public string? DataJson { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
