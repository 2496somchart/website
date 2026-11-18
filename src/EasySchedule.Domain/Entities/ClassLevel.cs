namespace EasySchedule.Domain.Entities;

/// <summary>
/// ระดับชั้น. Code: 11-16 = ป.1-ป.6, 21-23 = ม.1-ม.3, 34-36 = ม.4-ม.6,
/// ลงท้าย 0 (10/20/30) = รหัสรวมช่วงชั้น ไม่ใช่ห้องจริง, 99 = การกันเวลาครู
/// </summary>
public class ClassLevel : TermEntity
{
    public const int TeacherReservationCode = 99;

    public int Code { get; set; }
    public required string Name { get; set; }
    public int StartPeriod { get; set; } = 1;
    public int EndPeriod { get; set; } = 8;
    public List<ClassSection> Sections { get; set; } = [];

    /// <summary>รหัสรวมช่วงชั้น (10, 20, 30) — คำนวณจาก Code ไม่เก็บซ้ำ</summary>
    public bool IsAggregate => Code % 10 == 0;
}
