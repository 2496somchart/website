namespace EasySchedule.Domain.Entities;

/// <summary>รายวิชา (= TblClassSubjects)</summary>
public class Subject : TermEntity
{
    public required string Code { get; set; }
    public required string Title { get; set; }

    /// <summary>กลุ่มสาระ (G) เช่น "2" คณิต, "3" วิทย์, "8" อังกฤษ, "A" กิจกรรม</summary>
    public required string SubjectGroup { get; set; }

    public decimal Credits { get; set; }

    /// <summary>คาบต่อสัปดาห์ (ค่าเริ่มต้นของ Lesson)</summary>
    public int PPW { get; set; }

    /// <summary>คาบต่อครั้ง (ค่าเริ่มต้นของ Lesson)</summary>
    public int PT { get; set; } = 1;
}
