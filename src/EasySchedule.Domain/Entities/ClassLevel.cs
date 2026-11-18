namespace EasySchedule.Domain.Entities;

/// <summary>ระดับชั้น เช่น ป.1, ม.1, ม.4</summary>
public class ClassLevel : TenantEntity
{
    public required string Code { get; set; }
    public required string Name { get; set; }
    public int SortOrder { get; set; }
    public int ScheduleModeId { get; set; }
    public ScheduleMode? ScheduleMode { get; set; }
    public List<ClassSection> Sections { get; set; } = [];
}
