using EasySchedule.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EasySchedule.Infrastructure.Persistence;

internal sealed class SchoolConfig : IEntityTypeConfiguration<School>
{
    public void Configure(EntityTypeBuilder<School> e)
    {
        e.Property(x => x.Name).HasMaxLength(200);
        e.Property(x => x.SchCode).HasMaxLength(32);
        e.Property(x => x.Plan).HasConversion<string>().HasMaxLength(16);
        e.HasIndex(x => x.SchCode).IsUnique();
    }
}

internal sealed class TermConfig : IEntityTypeConfiguration<Term>
{
    public void Configure(EntityTypeBuilder<Term> e) =>
        e.HasIndex(x => new { x.SchoolId, x.Year, x.TermNo }).IsUnique();
}

internal sealed class ScheduleModeConfig : IEntityTypeConfiguration<ScheduleMode>
{
    public void Configure(EntityTypeBuilder<ScheduleMode> e)
    {
        e.HasIndex(x => new { x.TermId, x.Mode }).IsUnique();
        e.HasMany(x => x.Periods).WithOne(p => p.ScheduleMode).HasForeignKey(p => p.ScheduleModeId);
    }
}

internal sealed class PeriodConfig : IEntityTypeConfiguration<Period>
{
    public void Configure(EntityTypeBuilder<Period> e) =>
        e.HasIndex(x => new { x.ScheduleModeId, x.PeriodNo }).IsUnique();
}

internal sealed class ClassLevelConfig : IEntityTypeConfiguration<ClassLevel>
{
    public void Configure(EntityTypeBuilder<ClassLevel> e)
    {
        e.Property(x => x.Name).HasMaxLength(100);
        e.HasIndex(x => new { x.TermId, x.Code }).IsUnique();
        e.HasMany(x => x.Sections).WithOne(s => s.Level).HasForeignKey(s => s.LevelId);
        e.Ignore(x => x.IsAggregate);
    }
}

internal sealed class ClassSectionConfig : IEntityTypeConfiguration<ClassSection>
{
    public void Configure(EntityTypeBuilder<ClassSection> e) =>
        e.HasIndex(x => new { x.LevelId, x.SectionNo }).IsUnique();
}

internal sealed class BreakSlotConfig : IEntityTypeConfiguration<BreakSlot>
{
    public void Configure(EntityTypeBuilder<BreakSlot> e) =>
        e.HasIndex(x => new { x.ScheduleModeId, x.SectionId, x.DayNo, x.PeriodNo }).IsUnique();
}

internal sealed class TeacherConfig : IEntityTypeConfiguration<Teacher>
{
    public void Configure(EntityTypeBuilder<Teacher> e)
    {
        e.Property(x => x.Code).HasMaxLength(32);
        e.Property(x => x.Name).HasMaxLength(200);
        e.HasIndex(x => new { x.TermId, x.Code }).IsUnique();
    }
}

internal sealed class RoomConfig : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> e)
    {
        e.Property(x => x.Code).HasMaxLength(32);
        e.Property(x => x.Name).HasMaxLength(100);
        e.Property(x => x.Type).HasConversion<string>().HasMaxLength(1);
        e.HasIndex(x => new { x.TermId, x.Code }).IsUnique();
    }
}

internal sealed class SubjectConfig : IEntityTypeConfiguration<Subject>
{
    public void Configure(EntityTypeBuilder<Subject> e)
    {
        e.Property(x => x.Code).HasMaxLength(32);
        e.Property(x => x.Title).HasMaxLength(200);
        e.Property(x => x.SubjectGroup).HasMaxLength(4);
        e.Property(x => x.Credits).HasPrecision(4, 1);
        e.HasIndex(x => new { x.TermId, x.Code }).IsUnique();
    }
}

internal sealed class StudyPlanConfig : IEntityTypeConfiguration<StudyPlan>
{
    public void Configure(EntityTypeBuilder<StudyPlan> e)
    {
        e.Property(x => x.Name).HasMaxLength(100);
        e.HasIndex(x => new { x.TermId, x.Name }).IsUnique();
    }
}

internal sealed class StudyPlanSectionConfig : IEntityTypeConfiguration<StudyPlanSection>
{
    public void Configure(EntityTypeBuilder<StudyPlanSection> e) => e.HasKey(x => new { x.StudyPlanId, x.SectionId });
}

internal sealed class LessonConfig : IEntityTypeConfiguration<Lesson>
{
    public void Configure(EntityTypeBuilder<Lesson> e)
    {
        e.HasOne(x => x.Room).WithMany().HasForeignKey(x => x.RoomId);
        e.HasOne(x => x.Lab).WithMany().HasForeignKey(x => x.LabId);
        e.HasIndex(x => new { x.SectionId, x.SubjectId });
    }
}

internal sealed class LessonTeacherConfig : IEntityTypeConfiguration<LessonTeacher>
{
    public void Configure(EntityTypeBuilder<LessonTeacher> e)
    {
        e.HasKey(x => new { x.LessonId, x.TeacherId });
        e.HasIndex(x => new { x.LessonId, x.Ordinal }).IsUnique();
    }
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
    public void Configure(EntityTypeBuilder<SubjectRule> e)
    {
        e.Property(x => x.Type).HasConversion<string>().HasMaxLength(1);
        e.HasOne(x => x.Subject1).WithMany().HasForeignKey(x => x.Subject1Id);
        e.HasOne(x => x.Subject2).WithMany().HasForeignKey(x => x.Subject2Id);
    }
}

internal sealed class ReservationConfig : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> e)
    {
        e.Property(x => x.Note).HasMaxLength(200);
        e.ToTable(t => t.HasCheckConstraint("CK_Reservation_Target",
            "[StudyPlanId] IS NOT NULL OR [TeacherId] IS NOT NULL OR [RoomId] IS NOT NULL"));
    }
}

internal sealed class SnapshotConfig : IEntityTypeConfiguration<Snapshot>
{
    public void Configure(EntityTypeBuilder<Snapshot> e)
    {
        e.Property(x => x.Note).HasMaxLength(500);
        e.HasOne(x => x.Mode).WithMany().HasForeignKey(x => x.ModeId);
        e.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedBy);
    }
}

internal sealed class PlacementConfig : IEntityTypeConfiguration<Placement>
{
    public void Configure(EntityTypeBuilder<Placement> e) =>
        e.HasIndex(x => new { x.SnapshotId, x.DayNo, x.PeriodNo });
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
