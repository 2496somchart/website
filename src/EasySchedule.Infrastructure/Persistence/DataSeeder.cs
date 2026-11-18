using EasySchedule.Domain.Entities;
using EasySchedule.Domain.Enums;
using EasySchedule.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EasySchedule.Infrastructure.Persistence;

/// <summary>ข้อมูลตัวอย่าง: โรงเรียนเล็ก 1 แห่ง (ป.1, ม.1, ม.4 ระดับละ 2 ห้อง, ครู 10, วิชา 8)</summary>
public sealed class DataSeeder(EasyScheduleDbContext db, IPasswordService passwords, ILogger<DataSeeder> logger)
{
    public const string SampleSchoolCode = "DEMO01";
    public static readonly string[] SampleUsernames = ["superadmin", "admin", "viewer"];

    public async Task SeedAsync(string? userPassword, CancellationToken ct = default)
    {
        if (!db.BypassTenantFilter)
            throw new InvalidOperationException("DataSeeder must run with SystemTenantContext.");

        var school = await db.Schools.FirstOrDefaultAsync(s => s.Code == SampleSchoolCode, ct);
        if (school is null)
        {
            school = new School { Code = SampleSchoolCode, Name = "โรงเรียนตัวอย่างวิทยา", Address = "อำเภอเมือง จังหวัดตัวอย่าง" };
            db.Schools.Add(school);
            await db.SaveChangesAsync(ct);
            await SeedSchoolDataAsync(school.Id, ct);
            logger.LogInformation("Seeded sample school {Code} (id {Id})", school.Code, school.Id);
        }

        await SeedUsersAsync(school.Id, userPassword, ct);
    }

    private async Task SeedSchoolDataAsync(int schoolId, CancellationToken ct)
    {
        db.Terms.Add(new Term { SchoolId = schoolId, AcademicYear = 2569, Semester = 1, Name = "ภาคเรียนที่ 1/2569", IsActive = true });

        var mode = new ScheduleMode { SchoolId = schoolId, Name = "ตารางปกติ", DaysPerWeek = 5, PeriodsPerDay = 8 };
        (int n, string s, string e)[] periods =
        [
            (1, "08:30", "09:20"), (2, "09:20", "10:10"), (3, "10:20", "11:10"), (4, "11:10", "12:00"),
            (5, "13:00", "13:50"), (6, "13:50", "14:40"), (7, "14:40", "15:30"), (8, "15:30", "16:20"),
        ];
        foreach (var p in periods)
            mode.Periods.Add(new Period { SchoolId = schoolId, Number = p.n, StartTime = TimeOnly.Parse(p.s), EndTime = TimeOnly.Parse(p.e) });
        db.ScheduleModes.Add(mode);

        // BreakSlot ค่าเริ่มต้นตรงกันทั้งโรงเรียน (ClassLevelId = null, ทุกวัน)
        db.BreakSlots.AddRange(
            new BreakSlot { SchoolId = schoolId, Name = "พักเบรก", AfterPeriod = 2, StartTime = new(10, 10), EndTime = new(10, 20) },
            new BreakSlot { SchoolId = schoolId, Name = "พักกลางวัน", AfterPeriod = 4, StartTime = new(12, 0), EndTime = new(13, 0) });

        (string code, string name, int order)[] levels = [("P1", "ป.1", 1), ("M1", "ม.1", 7), ("M4", "ม.4", 10)];
        foreach (var l in levels)
        {
            var level = new ClassLevel { SchoolId = schoolId, Code = l.code, Name = l.name, SortOrder = l.order, ScheduleMode = mode };
            for (var i = 1; i <= 2; i++)
                level.Sections.Add(new ClassSection { SchoolId = schoolId, Number = i, Name = $"{l.name}/{i}" });
            db.ClassLevels.Add(level);
        }

        (string first, string last, string dept)[] teachers =
        [
            ("สมชาย", "ใจดี", "ภาษาไทย"), ("สมหญิง", "รักเรียน", "คณิตศาสตร์"), ("วิชัย", "เก่งกล้า", "วิทยาศาสตร์และเทคโนโลยี"),
            ("มาลี", "ศรีสุข", "สังคมศึกษา ศาสนา และวัฒนธรรม"), ("ประเสริฐ", "มั่นคง", "สุขศึกษาและพลศึกษา"),
            ("กมลา", "งามตา", "ศิลปะ"), ("ธนา", "พากเพียร", "การงานอาชีพ"), ("อรุณี", "แสงทอง", "ภาษาต่างประเทศ"),
            ("ณัฐพล", "บุญมา", "คณิตศาสตร์"), ("ปิยะนุช", "ทองดี", "ภาษาไทย"),
        ];
        for (var i = 0; i < teachers.Length; i++)
        {
            var t = teachers[i];
            db.Teachers.Add(new Teacher
            {
                SchoolId = schoolId, Code = $"T{i + 1:00}", Prefix = i % 2 == 0 ? "นาย" : "นาง",
                FirstName = t.first, LastName = t.last, Department = t.dept, MaxPeriodsPerWeek = 22,
            });
        }

        (string code, string name, string area, string color)[] subjects =
        [
            ("TH", "ภาษาไทย", "ภาษาไทย", "#ef4444"),
            ("MA", "คณิตศาสตร์", "คณิตศาสตร์", "#3b82f6"),
            ("SC", "วิทยาศาสตร์และเทคโนโลยี", "วิทยาศาสตร์และเทคโนโลยี", "#22c55e"),
            ("SO", "สังคมศึกษา ศาสนา และวัฒนธรรม", "สังคมศึกษา ศาสนา และวัฒนธรรม", "#eab308"),
            ("PE", "สุขศึกษาและพลศึกษา", "สุขศึกษาและพลศึกษา", "#f97316"),
            ("AR", "ศิลปะ", "ศิลปะ", "#a855f7"),
            ("CA", "การงานอาชีพ", "การงานอาชีพ", "#14b8a6"),
            ("EN", "ภาษาอังกฤษ", "ภาษาต่างประเทศ", "#ec4899"),
        ];
        foreach (var s in subjects)
            db.Subjects.Add(new Subject { SchoolId = schoolId, Code = s.code, Name = s.name, LearningArea = s.area, Color = s.color });

        db.Rooms.AddRange(
            new Room { SchoolId = schoolId, Code = "SCI1", Name = "ห้องปฏิบัติการวิทยาศาสตร์", RoomType = "lab" },
            new Room { SchoolId = schoolId, Code = "COM1", Name = "ห้องคอมพิวเตอร์", RoomType = "computer" });

        await db.SaveChangesAsync(ct);
    }

    private async Task SeedUsersAsync(int schoolId, string? password, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning("Seed:UserPassword is not set — sample users were not created.");
            return;
        }

        (string username, string display, UserRole role, int? school)[] users =
        [
            ("superadmin", "ผู้ดูแลระบบกลาง", UserRole.SuperAdmin, null),
            ("admin", "ผู้ดูแลโรงเรียนตัวอย่าง", UserRole.SchoolAdmin, schoolId),
            ("viewer", "ผู้ชมโรงเรียนตัวอย่าง", UserRole.Viewer, schoolId),
        ];
        foreach (var u in users)
        {
            if (await db.Users.AnyAsync(x => x.Username == u.username, ct)) continue;
            var user = new User { Username = u.username, DisplayName = u.display, Role = u.role, SchoolId = u.school, PasswordHash = "" };
            user.PasswordHash = passwords.Hash(user, password);
            db.Users.Add(user);
        }
        await db.SaveChangesAsync(ct);
    }
}
