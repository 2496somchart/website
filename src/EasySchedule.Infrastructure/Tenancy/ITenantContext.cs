namespace EasySchedule.Infrastructure.Tenancy;

/// <summary>บอก DbContext ว่าคำขอปัจจุบันเป็นของโรงเรียนไหน</summary>
public interface ITenantContext
{
    /// <summary>โรงเรียนของผู้ใช้ปัจจุบัน (null = ไม่ทราบ → ไม่เห็นข้อมูลใดเลย)</summary>
    int? SchoolId { get; }

    /// <summary>true เฉพาะ SuperAdmin และงานระบบ (migration/seed) — ข้าม filter ได้</summary>
    bool BypassFilter { get; }
}

/// <summary>ใช้ตอน seed/งานเบื้องหลัง: เห็นทุกโรงเรียน</summary>
public sealed class SystemTenantContext : ITenantContext
{
    public static readonly SystemTenantContext Instance = new();
    public int? SchoolId => null;
    public bool BypassFilter => true;
}

/// <summary>ใช้ใน test หรือโค้ดที่ต้องกำหนดโรงเรียนเอง</summary>
public sealed class FixedTenantContext(int? schoolId, bool bypassFilter = false) : ITenantContext
{
    public int? SchoolId { get; } = schoolId;
    public bool BypassFilter { get; } = bypassFilter;
}
