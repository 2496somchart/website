namespace EasySchedule.Domain.Entities;

public class Room : TenantEntity
{
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? Building { get; set; }
    public int? Capacity { get; set; }
    public string? RoomType { get; set; }
}
