namespace EasySchedule.Domain.Entities;

/// <summary>ครู (= TblTeacher). ชื่อจาก Importer เป็นชื่อสมมติ (ครู 001) เสมอ</summary>
public class Teacher : TermEntity
{
    public const int DefaultMaxConsecPeriods = 2;

    public required string Code { get; set; }
    public required string Name { get; set; }

    /// <summary>สอนติดกันได้ไม่เกิน (H4), 0 = ปิดการตรวจ</summary>
    public int MaxConsecPeriods { get; set; } = DefaultMaxConsecPeriods;
}
