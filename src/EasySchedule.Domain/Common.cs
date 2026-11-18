using EasySchedule.Domain.Entities;

namespace EasySchedule.Domain;

/// <summary>Entity ที่ผูกกับโรงเรียน — ถูกกรองด้วย global query filter ตาม SchoolId เสมอ</summary>
public interface ITenantEntity
{
    int SchoolId { get; set; }
}

public abstract class TenantEntity : ITenantEntity
{
    public int Id { get; set; }
    public int SchoolId { get; set; }
    public School? School { get; set; }
}
