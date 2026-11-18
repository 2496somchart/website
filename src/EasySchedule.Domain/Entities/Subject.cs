using EasySchedule.Domain.Enums;

namespace EasySchedule.Domain.Entities;

/// <summary>รายวิชา เช่น ท11101 ภาษาไทย</summary>
public class Subject : TenantEntity
{
    public required string Code { get; set; }
    public required string Name { get; set; }
    public string? ShortName { get; set; }
    public string? LearningArea { get; set; }
    public LoadType DefaultLoadType { get; set; } = LoadType.Teaching;
    public string? Color { get; set; }
}
