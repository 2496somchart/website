namespace EasySchedule.Domain.Entities;

/// <summary>กฎการจัดวิชา เช่น ไม่เกิน N คาบ/วัน, ห้ามลงคาบบ่าย</summary>
public class SubjectRule : TenantEntity
{
    public int SubjectId { get; set; }
    public Subject? Subject { get; set; }
    public required string RuleType { get; set; }
    public int? MaxPerDay { get; set; }
    /// <summary>พารามิเตอร์เพิ่มเติมของกฎในรูป JSON</summary>
    public string? ParametersJson { get; set; }
}
