using EasySchedule.Domain.Entities;
using EasySchedule.Domain.Enums;
using EasySchedule.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EasySchedule.Infrastructure.Persistence;

/// <summary>
/// ข้อมูลตัวอย่าง: โรงเรียนเล็ก 1 แห่ง — ป.1, ม.1, ม.4 ระดับละ 2 ห้อง, ครู 10 (ชื่อสมมติ), วิชา 8,
/// โหมด 0 และ 1, BreakSlot ค่าเริ่มต้นตรงกันทั้งโรงเรียน
/// </summary>
public sealed class DataSeeder(EasyScheduleDbContext db, IPasswordService passwords, ILogger<DataSeeder> logger)
{
    public const string SampleSchCode = "DEMO01";

    /// <summary>คาบพักค่าเริ่มต้น (PeriodNo) ของแต่ละโหมด — เป็นข้อมูล seed แก้ได้ทีหลังรายห้อง-รายวัน</summary>
    public const int DefaultBreakPeriodMode0 = 5;
    public const int DefaultBreakPeriodMode1 = 6;

    public async Task SeedAsync(string? userPassword, CancellationToken ct = default)
    {
        if (!db.BypassTenantFilter)
            throw new InvalidOperationException("DataSeeder must run with SystemTenantContext.");

        var school = await db.Schools.FirstOrDefaultAsync(s => s.SchCode == SampleSchCode, ct);
        if (school is null)
        {
            school = new School { SchCode = SampleSchCode, Name = "โรงเรียนตัวอย่างวิทยา", Plan = LicensePlan.Trial, MaxSectionsPerLevel = 4 };
            db.Schools.Add(school);
            await db.SaveChangesAsync(ct);
            await SeedSchoolDataAsync(school.Id, ct);
            logger.LogInformation("Seeded sample school {Code} (id {Id})", school.SchCode, school.Id);
        }

        await SeedUsersAsync(school.Id, userPassword, ct);
    }

    private async Task SeedSchoolDataAsync(int schoolId, CancellationToken ct)
    {
        var term = new Term { SchoolId = schoolId, Year = 2569, TermNo = 1, IsActive = true };
        db.Terms.Add(term);

        // โหมด 0: 5 วัน × 8 คาบ (+ P0 โฮมรูม)
        var mode0 = new ScheduleMode { SchoolId = schoolId, Term = term, Mode = ScheduleModeType.Normal, DaysList = [2, 3, 4, 5, 6], PeriodPerDay = 8 };
        AddPeriods(mode0, schoolId, term, new TimeOnly(8, 0), homeroomMinutes: 20, firstStart: new TimeOnly(8, 30), minutes: 50, count: 8);

        // โหมด 1: 4 วัน × 10 คาบ + วัน ร.ด. 1 วัน
        var mode1 = new ScheduleMode { SchoolId = schoolId, Term = term, Mode = ScheduleModeType.Military, DaysList = [2, 3, 4, 5, 6], PeriodPerDay = 10, MilitaryDay = 3 };
        AddPeriods(mode1, schoolId, term, new TimeOnly(8, 0), homeroomMinutes: 20, firstStart: new TimeOnly(8, 20), minutes: 40, count: 10);
        db.ScheduleModes.AddRange(mode0, mode1);

        (int code, string name)[] levels = [(11, "ป.1"), (21, "ม.1"), (34, "ม.4")];
        var sections = new List<ClassSection>();
        foreach (var (code, name) in levels)
        {
            var level = new ClassLevel { SchoolId = schoolId, Term = term, Code = code, Name = name, StartPeriod = 1, EndPeriod = 10 };
            for (var no = 1; no <= 2; no++)
            {
                var section = new ClassSection { SchoolId = schoolId, Term = term, SectionNo = no };
                level.Sections.Add(section);
                sections.Add(section);
            }
            db.ClassLevels.Add(level);
        }

        // BreakSlot ค่าเริ่มต้นตรงกันทั้งโรงเรียน: ทุกห้อง ทุกวัน
        foreach (var section in sections)
        {
            foreach (var day in mode0.DaysList)
            {
                db.BreakSlots.Add(new BreakSlot { SchoolId = schoolId, Term = term, ScheduleMode = mode0, Section = section, DayNo = day, PeriodNo = DefaultBreakPeriodMode0 });
                db.BreakSlots.Add(new BreakSlot { SchoolId = schoolId, Term = term, ScheduleMode = mode1, Section = section, DayNo = day, PeriodNo = DefaultBreakPeriodMode1 });
            }
        }

        // ครูใช้ชื่อสมมติเสมอ (CLAUDE.md กฎข้อ 10)
        for (var i = 1; i <= 10; i++)
            db.Teachers.Add(new Teacher { SchoolId = schoolId, Term = term, Code = $"T{i:000}", Name = $"ครู {i:000}" });

        (string code, string title, string g, int ppw)[] subjects =
        [
            ("ท21101", "ภาษาไทย", "1", 3),
            ("ค21101", "คณิตศาสตร์", "2", 3),
            ("ว21101", "วิทยาศาสตร์และเทคโนโลยี", "3", 3),
            ("ส21101", "สังคมศึกษา ศาสนา และวัฒนธรรม", "4", 3),
            ("พ21101", "สุขศึกษาและพลศึกษา", "5", 2),
            ("ศ21101", "ศิลปะ", "6", 2),
            ("ง21101", "การงานอาชีพ", "7", 2),
            ("อ21101", "ภาษาอังกฤษ", "8", 3),
        ];
        foreach (var (code, title, g, ppw) in subjects)
            db.Subjects.Add(new Subject { SchoolId = schoolId, Term = term, Code = code, Title = title, SubjectGroup = g, Credits = ppw * 0.5m, PPW = ppw, PT = 1 });

        db.Rooms.AddRange(
            new Room { SchoolId = schoolId, Term = term, Code = "R101", Name = "ห้อง 101", Type = RoomType.R, Capacity = 40 },
            new Room { SchoolId = schoolId, Term = term, Code = "L201", Name = "ห้องปฏิบัติการวิทยาศาสตร์", Type = RoomType.L, Capacity = 40 });

        // แผนการเรียนเป็นตารางลูก: ม.4/1 = วิทย์-คณิต, ม.4/2 = ศิลป์-ภาษา
        var m4 = sections.Where(s => s.Level!.Code == 34).OrderBy(s => s.SectionNo).ToList();
        var sciMath = new StudyPlan { SchoolId = schoolId, Term = term, Name = "วิทย์-คณิต" };
        sciMath.Sections.Add(new StudyPlanSection { SchoolId = schoolId, Section = m4[0] });
        var artLang = new StudyPlan { SchoolId = schoolId, Term = term, Name = "ศิลป์-ภาษา" };
        artLang.Sections.Add(new StudyPlanSection { SchoolId = schoolId, Section = m4[1] });
        db.StudyPlans.AddRange(sciMath, artLang);

        await db.SaveChangesAsync(ct);
    }

    private static void AddPeriods(ScheduleMode mode, int schoolId, Term term, TimeOnly homeroomStart, int homeroomMinutes, TimeOnly firstStart, int minutes, int count)
    {
        mode.Periods.Add(new Period { SchoolId = schoolId, Term = term, PeriodNo = 0, Start = homeroomStart, End = homeroomStart.AddMinutes(homeroomMinutes) });
        for (var p = 1; p <= count; p++)
        {
            var start = firstStart.AddMinutes((p - 1) * minutes);
            mode.Periods.Add(new Period { SchoolId = schoolId, Term = term, PeriodNo = p, Start = start, End = start.AddMinutes(minutes) });
        }
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
            ("admin", "ฝ่ายวิชาการ โรงเรียนตัวอย่าง", UserRole.SchoolAdmin, schoolId),
            ("viewer", "ครู โรงเรียนตัวอย่าง", UserRole.Viewer, schoolId),
        ];
        foreach (var (username, display, role, school) in users)
        {
            if (await db.Users.AnyAsync(x => x.Username == username, ct)) continue;
            var user = new User { Username = username, DisplayName = display, Role = role, SchoolId = school, PasswordHash = "" };
            user.PasswordHash = passwords.Hash(user, password);
            db.Users.Add(user);
        }
        await db.SaveChangesAsync(ct);
    }
}
