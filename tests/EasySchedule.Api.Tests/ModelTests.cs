using EasySchedule.Domain;
using EasySchedule.Domain.Entities;
using EasySchedule.Domain.Enums;
using EasySchedule.Infrastructure.Persistence;
using EasySchedule.Infrastructure.Tenancy;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace EasySchedule.Api.Tests;

[Collection(ApiCollection.Name)]
public sealed class ModelTests(ApiFixture fx)
{
    [Fact]
    public void Every_tenant_entity_has_a_query_filter()
    {
        using var db = fx.CreateDb(SystemTenantContext.Instance);
        var tenantTypes = db.Model.GetEntityTypes().Where(t => typeof(ITenantEntity).IsAssignableFrom(t.ClrType)).ToList();
        tenantTypes.Should().HaveCountGreaterThanOrEqualTo(18);
        tenantTypes.Should().OnlyContain(t => t.GetQueryFilter() != null);
    }

    [Fact]
    public void Every_scheduling_table_has_school_and_term()
    {
        // SPEC ข้อ 3: ทุกตารางมี SchoolId + TermId (ยกเว้น School/Term เอง, ตารางเชื่อม และ User/AuditLog)
        using var db = fx.CreateDb(SystemTenantContext.Instance);
        string[] exempt = [nameof(School), nameof(Term), nameof(User), nameof(AuditLog), nameof(LessonTeacher), nameof(StudyPlanSection)];
        var missing = db.Model.GetEntityTypes()
            .Where(t => !exempt.Contains(t.ClrType.Name))
            .Where(t => t.FindProperty("SchoolId") is null || t.FindProperty("TermId") is null)
            .Select(t => t.ClrType.Name);
        missing.Should().BeEmpty();
    }

    [Fact]
    public void Cascade_deletes_are_only_the_intended_ones()
    {
        using var db = fx.CreateDb(SystemTenantContext.Instance);
        var cascades = db.Model.GetEntityTypes().SelectMany(t => t.GetForeignKeys())
            .Where(fk => fk.DeleteBehavior == DeleteBehavior.Cascade)
            .Select(fk => $"{fk.DeclaringEntityType.ClrType.Name}->{fk.PrincipalEntityType.ClrType.Name}");
        cascades.Should().BeEquivalentTo(
            "BreakSlot->ClassSection", "Lesson->ClassSection", "LessonTeacher->Lesson",
            "Placement->Lesson", "Placement->Snapshot", "StudyPlanSection->ClassSection", "StudyPlanSection->StudyPlan");
    }

    [Fact]
    public async Task Deleting_snapshot_deletes_its_placements_only()
    {
        var (schoolId, termId, section, subject, mode) = await SampleAsync();
        await using var db = fx.CreateDb(SystemTenantContext.Instance);
        var lesson = new Lesson { SchoolId = schoolId, TermId = termId, SectionId = section, SubjectId = subject, PPW = 2 };
        var snap = new Snapshot { SchoolId = schoolId, TermId = termId, ModeId = mode, Note = "ฉบับทดสอบ" };
        snap.Placements.Add(new Placement { SchoolId = schoolId, TermId = termId, Lesson = lesson, DayNo = 2, PeriodNo = 1 });
        db.Snapshots.Add(snap);
        await db.SaveChangesAsync();

        db.Snapshots.Remove(snap);
        await db.SaveChangesAsync();

        (await db.Placements.AnyAsync(p => p.SnapshotId == snap.Id)).Should().BeFalse();
        (await db.Lessons.AnyAsync(l => l.Id == lesson.Id)).Should().BeTrue();
    }

    [Fact]
    public async Task Deleting_class_section_removes_its_lessons_placements_and_breaks_in_one_transaction()
    {
        // SPEC ข้อ 11: ลดจำนวนห้องแล้วต้องไม่มีภาระสอน/placement ค้าง
        var (schoolId, termId, _, subject, mode) = await SampleAsync();
        await using var db = fx.CreateDb(SystemTenantContext.Instance);
        var levelId = await db.ClassLevels.Where(l => l.SchoolId == schoolId && l.Code == 11).Select(l => l.Id).SingleAsync();
        var section = new ClassSection { SchoolId = schoolId, TermId = termId, LevelId = levelId, SectionNo = 9 };
        var lesson = new Lesson { SchoolId = schoolId, TermId = termId, Section = section, SubjectId = subject, PPW = 1 };
        var snap = new Snapshot { SchoolId = schoolId, TermId = termId, ModeId = mode };
        snap.Placements.Add(new Placement { SchoolId = schoolId, TermId = termId, Lesson = lesson, DayNo = 3, PeriodNo = 2 });
        db.Snapshots.Add(snap);
        db.BreakSlots.Add(new BreakSlot { SchoolId = schoolId, TermId = termId, ScheduleModeId = mode, Section = section, DayNo = 2, PeriodNo = 5 });
        await db.SaveChangesAsync();

        db.ClassSections.Remove(section);
        await db.SaveChangesAsync();

        (await db.Lessons.AnyAsync(l => l.Id == lesson.Id)).Should().BeFalse();
        (await db.Placements.AnyAsync(p => p.SnapshotId == snap.Id)).Should().BeFalse();
        (await db.BreakSlots.AnyAsync(b => b.SectionId == section.Id)).Should().BeFalse();
        (await db.Snapshots.AnyAsync(s => s.Id == snap.Id)).Should().BeTrue();
    }

    [Fact]
    public async Task Reservation_requires_a_target()
    {
        var (schoolId, termId, _, _, _) = await SampleAsync();
        await using var db = fx.CreateDb(SystemTenantContext.Instance);
        db.Reservations.Add(new Reservation { SchoolId = schoolId, TermId = termId, DayNo = 2, PeriodNo = 1 });
        var act = () => db.SaveChangesAsync();
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task Seed_matches_spec_sample_school()
    {
        var (schoolId, _, _, _, _) = await SampleAsync();
        await using var db = fx.CreateDb(SystemTenantContext.Instance);

        var school = await db.Schools.SingleAsync(s => s.Id == schoolId);
        school.Plan.Should().Be(LicensePlan.Trial);
        school.MaxSectionsPerLevel.Should().Be(4);
        school.TrialPlacementCap.Should().Be(200);

        (await db.ClassSections.CountAsync(s => s.SchoolId == schoolId && s.SectionNo <= 2)).Should().Be(6);
        (await db.Teachers.Where(t => t.SchoolId == schoolId).Select(t => t.Name).ToListAsync())
            .Should().HaveCount(10).And.OnlyContain(n => n.StartsWith("ครู "));
        (await db.Subjects.CountAsync(s => s.SchoolId == schoolId)).Should().Be(8);

        var modes = await db.ScheduleModes.Include(m => m.Periods).Where(m => m.SchoolId == schoolId).ToListAsync();
        var m0 = modes.Single(m => m.Mode == ScheduleModeType.Normal);
        var m1 = modes.Single(m => m.Mode == ScheduleModeType.Military);
        m0.PeriodPerDay.Should().Be(8);
        m0.DaysList.Should().Equal(2, 3, 4, 5, 6);
        m0.Periods.Select(p => p.PeriodNo).Should().BeEquivalentTo(Enumerable.Range(0, 9));
        m1.PeriodPerDay.Should().Be(10);
        m1.MilitaryDay.Should().NotBeNull();

        // BreakSlot ค่าเริ่มต้นตรงกันทั้งโรงเรียน: ทุกห้อง × ทุกวัน คาบเดียวกัน
        var breaks0 = await db.BreakSlots.Where(b => b.SchoolId == schoolId && b.ScheduleModeId == m0.Id && b.Section!.SectionNo <= 2).ToListAsync();
        breaks0.Should().HaveCount(6 * 5);
        breaks0.Select(b => b.PeriodNo).Distinct().Should().ContainSingle().Which.Should().Be(DataSeeder.DefaultBreakPeriodMode0);

        (await db.StudyPlanSections.CountAsync(s => s.SchoolId == schoolId)).Should().Be(2);
    }

    private async Task<(int School, int Term, int Section, int Subject, int Mode)> SampleAsync()
    {
        var schoolId = await fx.SampleSchoolIdAsync();
        var termId = await fx.ActiveTermIdAsync(schoolId);
        await using var db = fx.CreateDb(SystemTenantContext.Instance);
        var section = await db.ClassSections.Where(s => s.SchoolId == schoolId).Select(s => s.Id).FirstAsync();
        var subject = await db.Subjects.Where(s => s.SchoolId == schoolId).Select(s => s.Id).FirstAsync();
        var mode = await db.ScheduleModes.Where(m => m.SchoolId == schoolId && m.Mode == ScheduleModeType.Normal).Select(m => m.Id).SingleAsync();
        return (schoolId, termId, section, subject, mode);
    }
}
