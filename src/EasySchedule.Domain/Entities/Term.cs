namespace EasySchedule.Domain.Entities;

/// <summary>ปี/เทอมของ<b>งานจัดตาราง</b> (= SchYr/SchTr เดิม) ไม่ใช่ปีทะเบียน (Yr/Tr)</summary>
public class Term : TenantEntity
{
    /// <summary>ปีการศึกษา พ.ศ. เช่น 2569</summary>
    public int Year { get; set; }

    /// <summary>เทอม 1 หรือ 2 (ชื่อ Term ใช้ไม่ได้เพราะซ้ำกับชื่อ class)</summary>
    public int TermNo { get; set; }

    public bool IsActive { get; set; }
}
