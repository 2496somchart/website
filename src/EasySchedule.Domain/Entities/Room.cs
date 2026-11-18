using EasySchedule.Domain.Enums;

namespace EasySchedule.Domain.Entities;

/// <summary>ห้อง/แล็บ (= TblRoom)</summary>
public class Room : TermEntity
{
    public required string Code { get; set; }
    public required string Name { get; set; }
    public RoomType Type { get; set; } = RoomType.R;
    public int? Capacity { get; set; }
}
