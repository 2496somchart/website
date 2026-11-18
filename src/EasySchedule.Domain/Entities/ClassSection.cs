namespace EasySchedule.Domain.Entities;

/// <summary>ห้องเรียน เช่น ม.1/1. จำนวนห้องมาจาก record จริง ไม่ใช่ตัวเลข ToClassRoomID</summary>
public class ClassSection : TermEntity
{
    public int LevelId { get; set; }
    public ClassLevel? Level { get; set; }
    public int SectionNo { get; set; }

    /// <summary>ห้องเรียนประจำ (Room)</summary>
    public int? HomeroomId { get; set; }
    public Room? Homeroom { get; set; }

    public List<StudyPlanSection> StudyPlans { get; set; } = [];
}
