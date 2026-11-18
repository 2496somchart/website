namespace EasySchedule.Domain.Enums;

/// <summary>
/// ประเภทภาระงานของคาบสอน (ค่า 1-7 ตาม SPEC ข้อ 3).
/// ชื่อแต่ละค่ายังเป็นชื่อกลาง ๆ เพราะตอนทำ M0 ยังไม่มีไฟล์ SPEC — ดู docs/DECISIONS.md (D-004)
/// </summary>
public enum LoadType
{
    Type1 = 1,
    Type2 = 2,
    Type3 = 3,
    Type4 = 4,
    Type5 = 5,
    Type6 = 6,
    Type7 = 7,
}

public enum UserRole
{
    SuperAdmin = 1,
    SchoolAdmin = 2,
    Viewer = 3,
}

/// <summary>วันในสัปดาห์ของตารางสอน (1 = จันทร์ … 7 = อาทิตย์)</summary>
public enum SchoolDay
{
    Monday = 1,
    Tuesday = 2,
    Wednesday = 3,
    Thursday = 4,
    Friday = 5,
    Saturday = 6,
    Sunday = 7,
}
