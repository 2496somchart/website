namespace EasySchedule.Domain.Entities;

public class School
{
    public int Id { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? Address { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
