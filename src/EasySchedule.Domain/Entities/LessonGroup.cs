namespace EasySchedule.Domain.Entities;

/// <summary>กลุ่ม Lesson ที่ต้องวางคาบเดียวกัน (team teaching, วิชาเลือกคู่ขนาน)</summary>
public class LessonGroup : TermEntity
{
    public required string Name { get; set; }
    public List<Lesson> Lessons { get; set; } = [];
}
