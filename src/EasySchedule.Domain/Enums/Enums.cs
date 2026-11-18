namespace EasySchedule.Domain.Enums;

/// <summary>
/// ประเภทภาระงาน (LoadTypeID 1-7) — คงค่าเดิมเพื่อ import จาก Sch69 ได้ตรง ห้ามเปลี่ยนตัวเลข
/// </summary>
public enum LoadType
{
    /// <summary>บริหาร — ภาระบริหาร ไม่ถูกจัดลงตาราง (H14)</summary>
    Administration = 1,
    /// <summary>กิจกรรม เช่น ร.ด.</summary>
    Activity = 2,
    /// <summary>สอนคณะ — การกันเวลาครู</summary>
    TeamTeaching = 3,
    /// <summary>ส่วนบุคคล — การกันเวลาครู (เช่น พบแพทย์)</summary>
    Personal = 4,
    /// <summary>สอน — วิชาปกติ</summary>
    Teaching = 5,
    /// <summary>เลือกเสรี — วิชาเลือก</summary>
    FreeElective = 6,
    /// <summary>พัก — คาบพัก (แยกจาก 5 เพื่อไม่ให้เข้ากฎ H10)</summary>
    Break = 7,
}

public static class LoadTypeExtensions
{
    public static string ToThai(this LoadType type) => type switch
    {
        LoadType.Administration => "บริหาร",
        LoadType.Activity => "กิจกรรม",
        LoadType.TeamTeaching => "สอนคณะ",
        LoadType.Personal => "ส่วนบุคคล",
        LoadType.Teaching => "สอน",
        LoadType.FreeElective => "เลือกเสรี",
        LoadType.Break => "พัก",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
    };

    /// <summary>LoadType 3/4 = การกันเวลาครู</summary>
    public static bool IsTeacherReservation(this LoadType type) => type is LoadType.TeamTeaching or LoadType.Personal;
}

/// <summary>ประเภทรายวิชาตามหลักสูตร</summary>
public enum SubjectCategory
{
    /// <summary>รายวิชาพื้นฐาน — เรียนตามเทอมที่กำหนดในแผน</summary>
    Basic = 1,
    /// <summary>รายวิชาเพิ่มเติม — โรงเรียนจัดเอง</summary>
    Additional = 2,
    /// <summary>รายวิชาเพิ่มเติมเลือก — นักเรียนเลือก</summary>
    AdditionalElective = 3,
    /// <summary>กิจกรรมพัฒนาผู้เรียน</summary>
    Activity = 4,
}

public static class SubjectCategoryExtensions
{
    public static string ToThai(this SubjectCategory c) => c switch
    {
        SubjectCategory.Basic => "พื้นฐาน",
        SubjectCategory.Additional => "เพิ่มเติม",
        SubjectCategory.AdditionalElective => "เพิ่มเติมเลือก",
        SubjectCategory.Activity => "กิจกรรมพัฒนาผู้เรียน",
        _ => throw new ArgumentOutOfRangeException(nameof(c), c, null),
    };
}

public enum UserRole
{
    SuperAdmin = 1,
    SchoolAdmin = 2,
    Viewer = 3,
}

/// <summary>0 = ปกติ (5 วัน × 8 คาบ), 1 = ร.ด. (4 วัน × 10 คาบ + วัน ร.ด.)</summary>
public enum ScheduleModeType
{
    Normal = 0,
    Military = 1,
}

public enum LicensePlan
{
    Trial = 0,
    Licensed = 1,
    Expired = 2,
}

/// <summary>ประเภทห้อง: R = ห้องเรียน, L = ห้องปฏิบัติการ</summary>
public enum RoomType
{
    R = 0,
    L = 1,
}

/// <summary>SubjectRule: C = ต้องต่อเนื่อง, U = ห้ามวันเดียวกัน, S = ต้องเรียงลำดับ</summary>
public enum SubjectRuleType
{
    C = 0,
    U = 1,
    S = 2,
}
