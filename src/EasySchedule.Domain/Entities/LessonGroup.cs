namespace EasySchedule.Domain.Entities;

/// <summary>กลุ่มคาบที่ต้องลงเวลาเดียวกัน เช่น วิชาเลือก/ชุมนุม ที่หลายห้องเรียนพร้อมกัน</summary>
public class LessonGroup : TenantEntity
{
    public int TermId { get; set; }
    public Term? Term { get; set; }
    public required string Name { get; set; }
    public List<Lesson> Lessons { get; set; } = [];
}
