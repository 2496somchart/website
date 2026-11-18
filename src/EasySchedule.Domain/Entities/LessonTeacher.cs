namespace EasySchedule.Domain.Entities;

/// <summary>ครูผู้สอนของ Lesson (= TblClassTeacher / TeacIDList). Ordinal 1 = อาจารย์หลัก</summary>
public class LessonTeacher : ITenantEntity
{
    public int SchoolId { get; set; }
    public int LessonId { get; set; }
    public Lesson? Lesson { get; set; }
    public int TeacherId { get; set; }
    public Teacher? Teacher { get; set; }
    public int Ordinal { get; set; } = 1;
}
