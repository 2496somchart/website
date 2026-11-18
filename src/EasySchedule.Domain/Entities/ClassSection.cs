namespace EasySchedule.Domain.Entities;

/// <summary>ห้องเรียน (กลุ่มนักเรียน) เช่น ม.1/1</summary>
public class ClassSection : TenantEntity
{
    public int ClassLevelId { get; set; }
    public ClassLevel? ClassLevel { get; set; }
    public int Number { get; set; }
    public required string Name { get; set; }
    public int? HomeroomTeacherId { get; set; }
    public Teacher? HomeroomTeacher { get; set; }
    public int? HomeRoomId { get; set; }
    public Room? HomeRoom { get; set; }
}
