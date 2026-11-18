using EasySchedule.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EasySchedule.Infrastructure.Persistence;

internal sealed class SchoolConfig : IEntityTypeConfiguration<School>
{
    public void Configure(EntityTypeBuilder<School> e)
    {
        e.Property(x => x.Code).HasMaxLength(32);
        e.Property(x => x.Name).HasMaxLength(200);
        e.Property(x => x.Address).HasMaxLength(500);
        e.HasIndex(x => x.Code).IsUnique();
    }
}

internal sealed class TermConfig : IEntityTypeConfiguration<Term>
{
    public void Configure(EntityTypeBuilder<Term> e)
    {
        e.Property(x => x.Name).HasMaxLength(100);
        e.HasIndex(x => new { x.SchoolId, x.AcademicYear, x.Semester }).IsUnique();
    }
}

internal sealed class ScheduleModeConfig : IEntityTypeConfiguration<ScheduleMode>
{
    public void Configure(EntityTypeBuilder<ScheduleMode> e)
    {
        e.Property(x => x.Name).HasMaxLength(100);
        e.HasMany(x => x.Periods).WithOne(p => p.ScheduleMode).HasForeignKey(p => p.ScheduleModeId);
    }
}

internal sealed class PeriodConfig : IEntityTypeConfiguration<Period>
{
    public void Configure(EntityTypeBuilder<Period> e) =>
        e.HasIndex(x => new { x.ScheduleModeId, x.Number }).IsUnique();
}

internal sealed class ClassLevelConfig : IEntityTypeConfiguration<ClassLevel>
{
    public void Configure(EntityTypeBuilder<ClassLevel> e)
    {
        e.Property(x => x.Code).HasMaxLength(32);
        e.Property(x => x.Name).HasMaxLength(100);
        e.HasIndex(x => new { x.SchoolId, x.Code }).IsUnique();
        e.HasMany(x => x.Sections).WithOne(s => s.ClassLevel).HasForeignKey(s => s.ClassLevelId);
    }
}

internal sealed class ClassSectionConfig : IEntityTypeConfiguration<ClassSection>
{
    public void Configure(EntityTypeBuilder<ClassSection> e)
    {
        e.Property(x => x.Name).HasMaxLength(100);
        e.HasIndex(x => new { x.ClassLevelId, x.Number }).IsUnique();
    }
}

internal sealed class BreakSlotConfig : IEntityTypeConfiguration<BreakSlot>
{
    public void Configure(EntityTypeBuilder<BreakSlot> e) => e.Property(x => x.Name).HasMaxLength(100);
}

internal sealed class TeacherConfig : IEntityTypeConfiguration<Teacher>
{
    public void Configure(EntityTypeBuilder<Teacher> e)
    {
        e.Property(x => x.Code).HasMaxLength(32);
        e.Property(x => x.Prefix).HasMaxLength(50);
        e.Property(x => x.FirstName).HasMaxLength(100);
        e.Property(x => x.LastName).HasMaxLength(100);
        e.Property(x => x.Department).HasMaxLength(100);
        e.HasIndex(x => new { x.SchoolId, x.Code }).IsUnique();
    }
}

internal sealed class RoomConfig : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> e)
    {
        e.Property(x => x.Code).HasMaxLength(32);
        e.Property(x => x.Name).HasMaxLength(100);
        e.Property(x => x.Building).HasMaxLength(100);
        e.Property(x => x.RoomType).HasMaxLength(50);
        e.HasIndex(x => new { x.SchoolId, x.Code }).IsUnique();
    }
}

internal sealed class SubjectConfig : IEntityTypeConfiguration<Subject>
{
    public void Configure(EntityTypeBuilder<Subject> e)
    {
        e.Property(x => x.Code).HasMaxLength(32);
        e.Property(x => x.Name).HasMaxLength(200);
        e.Property(x => x.ShortName).HasMaxLength(50);
        e.Property(x => x.LearningArea).HasMaxLength(100);
        e.Property(x => x.Color).HasMaxLength(16);
        e.HasIndex(x => new { x.SchoolId, x.Code }).IsUnique();
    }
}

internal sealed class StudyPlanConfig : IEntityTypeConfiguration<StudyPlan>
{
    public void Configure(EntityTypeBuilder<StudyPlan> e) =>
        e.HasIndex(x => new { x.TermId, x.ClassLevelId, x.SubjectId }).IsUnique();
}

internal sealed class LessonTeacherConfig : IEntityTypeConfiguration<LessonTeacher>
{
    public void Configure(EntityTypeBuilder<LessonTeacher> e) => e.HasKey(x => new { x.LessonId, x.TeacherId });
}

internal sealed class LessonGroupConfig : IEntityTypeConfiguration<LessonGroup>
{
    public void Configure(EntityTypeBuilder<LessonGroup> e)
    {
        e.Property(x => x.Name).HasMaxLength(100);
        e.HasMany(x => x.Lessons).WithOne(l => l.LessonGroup).HasForeignKey(l => l.LessonGroupId);
    }
}

internal sealed class SubjectRuleConfig : IEntityTypeConfiguration<SubjectRule>
{
    public void Configure(EntityTypeBuilder<SubjectRule> e) => e.Property(x => x.RuleType).HasMaxLength(50);
}

internal sealed class ReservationConfig : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> e) => e.Property(x => x.Reason).HasMaxLength(200);
}

internal sealed class SnapshotConfig : IEntityTypeConfiguration<Snapshot>
{
    public void Configure(EntityTypeBuilder<Snapshot> e) => e.Property(x => x.Name).HasMaxLength(100);
}

internal sealed class PlacementConfig : IEntityTypeConfiguration<Placement>
{
    public void Configure(EntityTypeBuilder<Placement> e) =>
        e.HasIndex(x => new { x.SnapshotId, x.Day, x.PeriodNumber });
}

internal sealed class UserConfig : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> e)
    {
        e.Property(x => x.Username).HasMaxLength(100);
        e.Property(x => x.DisplayName).HasMaxLength(200);
        e.Property(x => x.PasswordHash).HasMaxLength(500);
        e.HasIndex(x => x.Username).IsUnique();
    }
}

internal sealed class AuditLogConfig : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> e)
    {
        e.Property(x => x.Action).HasMaxLength(50);
        e.Property(x => x.EntityName).HasMaxLength(100);
        e.Property(x => x.EntityId).HasMaxLength(64);
        e.HasIndex(x => new { x.SchoolId, x.CreatedAt });
    }
}
