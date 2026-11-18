namespace EasySchedule.Domain.Entities;

/// <summary>ภาคเรียน เช่น ปีการศึกษา 2569 ภาคเรียนที่ 1</summary>
public class Term : TenantEntity
{
    public int AcademicYear { get; set; }
    public int Semester { get; set; }
    public required string Name { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public bool IsActive { get; set; }
}
