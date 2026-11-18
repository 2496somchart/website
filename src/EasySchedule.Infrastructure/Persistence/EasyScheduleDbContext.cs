using System.Linq.Expressions;
using EasySchedule.Domain;
using EasySchedule.Domain.Entities;
using EasySchedule.Infrastructure.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace EasySchedule.Infrastructure.Persistence;

public class EasyScheduleDbContext(DbContextOptions<EasyScheduleDbContext> options, ITenantContext tenant)
    : DbContext(options)
{
    public const string ThaiCollation = "Thai_100_CI_AS";

    // สองค่านี้ถูกอ่านใหม่ทุก query (EF ผูก filter กับ instance ของ DbContext)
    public int? CurrentSchoolId => tenant.SchoolId;
    public bool BypassTenantFilter => tenant.BypassFilter;

    public DbSet<School> Schools => Set<School>();
    public DbSet<Term> Terms => Set<Term>();
    public DbSet<ScheduleMode> ScheduleModes => Set<ScheduleMode>();
    public DbSet<Period> Periods => Set<Period>();
    public DbSet<ClassLevel> ClassLevels => Set<ClassLevel>();
    public DbSet<ClassSection> ClassSections => Set<ClassSection>();
    public DbSet<BreakSlot> BreakSlots => Set<BreakSlot>();
    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<StudyPlan> StudyPlans => Set<StudyPlan>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<LessonTeacher> LessonTeachers => Set<LessonTeacher>();
    public DbSet<LessonGroup> LessonGroups => Set<LessonGroup>();
    public DbSet<SubjectRule> SubjectRules => Set<SubjectRule>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<Snapshot> Snapshots => Set<Snapshot>();
    public DbSet<Placement> Placements => Set<Placement>();
    public DbSet<User> Users => Set<User>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.UseCollation(ThaiCollation);
        b.ApplyConfigurationsFromAssembly(typeof(EasyScheduleDbContext).Assembly);

        // ค่าเริ่มต้น: ห้ามลบแบบ cascade (กันลบข้อมูลโรงเรียนหายเป็นทอด ๆ)
        foreach (var fk in b.Model.GetEntityTypes().SelectMany(t => t.GetForeignKeys()))
        {
            if (!fk.IsOwnership)
                fk.DeleteBehavior = DeleteBehavior.Restrict;
        }

        // ข้อยกเว้นที่ตั้งใจ: Placement ลบตาม Snapshot และ Lesson, LessonTeacher ลบตาม Lesson
        b.Entity<Placement>().HasOne(p => p.Snapshot).WithMany(s => s.Placements)
            .HasForeignKey(p => p.SnapshotId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<Placement>().HasOne(p => p.Lesson).WithMany()
            .HasForeignKey(p => p.LessonId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<LessonTeacher>().HasOne(lt => lt.Lesson).WithMany(l => l.Teachers)
            .HasForeignKey(lt => lt.LessonId).OnDelete(DeleteBehavior.Cascade);

        // Global query filter ตาม SchoolId ทุก entity ที่เป็น ITenantEntity
        foreach (var type in b.Model.GetEntityTypes().Where(t => typeof(ITenantEntity).IsAssignableFrom(t.ClrType)))
        {
            typeof(EasyScheduleDbContext)
                .GetMethod(nameof(ApplyTenantFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .MakeGenericMethod(type.ClrType)
                .Invoke(this, [b]);
        }

        b.Entity<School>().HasQueryFilter(s => BypassTenantFilter || s.Id == CurrentSchoolId);
    }

    private void ApplyTenantFilter<T>(ModelBuilder b) where T : class, ITenantEntity
    {
        Expression<Func<T, bool>> filter = e => BypassTenantFilter || e.SchoolId == CurrentSchoolId;
        b.Entity<T>().HasQueryFilter(filter);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        EnforceTenantOnWrite();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        EnforceTenantOnWrite();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    /// <summary>เติม SchoolId ให้แถวใหม่ และห้ามเขียนข้อมูลของโรงเรียนอื่น</summary>
    private void EnforceTenantOnWrite()
    {
        if (BypassTenantFilter) return;

        foreach (var entry in ChangeTracker.Entries<ITenantEntity>())
        {
            if (entry.State == EntityState.Added && entry.Entity.SchoolId == 0 && CurrentSchoolId is int sid)
                entry.Entity.SchoolId = sid;

            if (entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted
                && entry.Entity.SchoolId != CurrentSchoolId)
            {
                throw new UnauthorizedAccessException(
                    $"Cannot write {entry.Metadata.ClrType.Name} of school {entry.Entity.SchoolId} from tenant {CurrentSchoolId?.ToString() ?? "<none>"}.");
            }
        }
    }
}
