namespace EasySchedule.Domain.Entities;

public class Teacher : TenantEntity
{
    public required string Code { get; set; }
    public string? Prefix { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? Department { get; set; }
    public int? MaxPeriodsPerWeek { get; set; }
    public bool IsActive { get; set; } = true;
}
