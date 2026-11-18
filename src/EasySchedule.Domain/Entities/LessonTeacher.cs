namespace EasySchedule.Domain.Entities;

/// <summary>ครูผู้สอนของ Lesson (หนึ่ง Lesson อาจมีครูหลายคน เช่น สอนร่วม)</summary>
public class LessonTeacher : ITenantEntity
{
    public int SchoolId { get; set; }
    public int LessonId { get; set; }
    public Lesson? Lesson { get; set; }
    public int TeacherId { get; set; }
    public Teacher? Teacher { get; set; }
    public bool IsPrimary { get; set; } = true;
}
