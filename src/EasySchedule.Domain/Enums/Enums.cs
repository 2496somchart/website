namespace EasySchedule.Domain.Enums;

/// <summary>
/// ประเภทภาระงานของครู (LoadTypeID 1-7 ตาม SPEC). ค่าตัวเลขถูกเก็บลงฐานข้อมูล ห้ามเปลี่ยน.
/// ชื่อภาษาไทยสำหรับแสดงผลอยู่ที่ <see cref="LoadTypeExtensions.ToThai"/>
/// </summary>
public enum LoadType
{
    /// <summary>บริหาร</summary>
    Administration = 1,
    /// <summary>กิจกรรม</summary>
    Activity = 2,
    /// <summary>สอนคณะ</summary>
    TeamTeaching = 3,
    /// <summary>ส่วนบุคคล</summary>
    Personal = 4,
    /// <summary>สอน</summary>
    Teaching = 5,
    /// <summary>เลือกเสรี</summary>
    FreeElective = 6,
    /// <summary>พัก</summary>
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
