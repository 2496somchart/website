namespace EasySchedule.Solver;

// โครงสัญญาของ solver (CLAUDE.md ข้อ 8) — ตัว implementation มาใน M3.
// project นี้ห้ามอ้าง EF Core / Infrastructure / DB ใดๆ (มี test ตรวจ)

/// <summary>ข้อมูลเข้าแบบ in-memory ที่ solver ใช้ (เติมฟิลด์ใน M3)</summary>
public sealed record SolverInput(int SchoolId, int TermId, int ModeId, int Seed, TimeSpan TimeLimit);

public sealed record PlacedLesson(int LessonId, int DayNo, int PeriodNo, int? RoomId, bool IsLocked);

/// <summary>คาบที่วางไม่ได้ พร้อมเหตุผลภาษาไทย</summary>
public sealed record UnplacedLesson(int LessonId, int Missing, string ReasonThai);

public sealed record SolverResult(IReadOnlyList<PlacedLesson> Placements, IReadOnlyList<UnplacedLesson> Unplaced);

public interface ISolver
{
    Task<SolverResult> SolveAsync(SolverInput input, IProgress<int>? progress = null, CancellationToken ct = default);
}
