using EasySchedule.Domain;
using EasySchedule.Domain.Entities;
using EasySchedule.Domain.Enums;
using EasySchedule.Infrastructure.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace EasySchedule.Tests;

[Collection(ApiCollection.Name)]
public sealed class ModelTests(ApiFixture fx)
{
    [Fact]
    public void Every_tenant_entity_has_a_query_filter()
    {
        using var db = fx.CreateDb(SystemTenantContext.Instance);
        var tenantTypes = db.Model.GetEntityTypes().Where(t => typeof(ITenantEntity).IsAssignableFrom(t.ClrType)).ToList();
        Assert.True(tenantTypes.Count >= 17);
        Assert.All(tenantTypes, t => Assert.NotNull(t.GetQueryFilter()));
    }

    [Fact]
    public void LoadType_has_values_1_to_7()
    {
        Assert.Equal(Enumerable.Range(1, 7), Enum.GetValues<LoadType>().Select(v => (int)v));
    }

    [Fact]
    public void Placement_cascades_from_snapshot_and_lesson_only()
    {
        using var db = fx.CreateDb(SystemTenantContext.Instance);
        var cascades = db.Model.GetEntityTypes().SelectMany(t => t.GetForeignKeys())
            .Where(fk => fk.DeleteBehavior == DeleteBehavior.Cascade)
            .Select(fk => $"{fk.DeclaringEntityType.ClrType.Name}->{fk.PrincipalEntityType.ClrType.Name}")
            .OrderBy(x => x).ToList();
        Assert.Equal(["LessonTeacher->Lesson", "Placement->Lesson", "Placement->Snapshot"], cascades);
    }

    [Fact]
    public async Task Deleting_snapshot_deletes_its_placements()
    {
        await using var db = fx.CreateDb(SystemTenantContext.Instance);
        var schoolId = await fx.SampleSchoolIdAsync();
        var term = await db.Terms.FirstAsync(t => t.SchoolId == schoolId);
        var subject = await db.Subjects.FirstAsync(s => s.SchoolId == schoolId);
        var section = await db.ClassSections.FirstAsync(s => s.SchoolId == schoolId);

        var lesson = new Lesson { SchoolId = schoolId, TermId = term.Id, SubjectId = subject.Id, ClassSectionId = section.Id, PeriodsPerWeek = 2 };
        var snap = new Snapshot { SchoolId = schoolId, TermId = term.Id, Name = "ฉบับทดสอบ" };
        snap.Placements.Add(new Placement { SchoolId = schoolId, Lesson = lesson, Day = SchoolDay.Monday, PeriodNumber = 1 });
        db.Snapshots.Add(snap);
        await db.SaveChangesAsync();

        db.Snapshots.Remove(snap);
        await db.SaveChangesAsync();

        Assert.False(await db.Placements.AnyAsync(p => p.SnapshotId == snap.Id));
        Assert.True(await db.Lessons.AnyAsync(l => l.Id == lesson.Id));
    }

    [Fact]
    public async Task Seed_has_expected_sample_school()
    {
        await using var db = fx.CreateDb(SystemTenantContext.Instance);
        var schoolId = await fx.SampleSchoolIdAsync();
        Assert.Equal(6, await db.ClassSections.CountAsync(s => s.SchoolId == schoolId));
        Assert.Equal(10, await db.Teachers.CountAsync(t => t.SchoolId == schoolId));
        Assert.Equal(8, await db.Subjects.CountAsync(s => s.SchoolId == schoolId));
        var breaks = await db.BreakSlots.Where(b => b.SchoolId == schoolId).ToListAsync();
        Assert.NotEmpty(breaks);
        Assert.All(breaks, b => { Assert.Null(b.ClassLevelId); Assert.Null(b.Day); });
    }
}
