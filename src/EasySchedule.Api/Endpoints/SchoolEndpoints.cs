using System.Security.Claims;
using EasySchedule.Api.Auth;
using EasySchedule.Domain.Entities;
using EasySchedule.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EasySchedule.Api.Endpoints;

public static class SchoolEndpoints
{
    public static void MapSchoolEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/schools/{schoolId:int}").WithTags("Schools");

        // ระดับชั้นของเทอมที่ระบุ (ไม่ระบุ = เทอมที่ IsActive)
        g.MapGet("/classlevels", async (int schoolId, int? termId, ClaimsPrincipal user, EasyScheduleDbContext db, CancellationToken ct) =>
        {
            if (!user.CanAccessSchool(schoolId)) return Results.Forbid();

            var term = await ResolveTermAsync(db, schoolId, termId, ct);
            if (term is null)
                return Results.Problem("ยังไม่ได้กำหนดปี/เทอมจัดตารางที่ใช้งานอยู่", statusCode: StatusCodes.Status404NotFound);

            var levels = await db.ClassLevels
                .Where(l => l.SchoolId == schoolId && l.TermId == term.Value)
                .OrderBy(l => l.Code)
                .Select(l => new
                {
                    l.Id,
                    l.Code,
                    l.Name,
                    l.StartPeriod,
                    l.EndPeriod,
                    Sections = l.Sections.OrderBy(s => s.SectionNo).Select(s => new { s.Id, s.SectionNo }).ToList(),
                })
                .ToListAsync(ct);

            return Results.Ok(levels.Select(l => new ClassLevelDto(l.Id, l.Code, l.Name, l.StartPeriod, l.EndPeriod,
                l.Sections.Select(s => new ClassSectionDto(s.Id, s.SectionNo, $"{l.Name}/{s.SectionNo}")).ToList())));
        }).RequireAuthorization(Policies.AnyUser);

        g.MapPost("/classlevels", async (int schoolId, CreateClassLevelRequest req, ClaimsPrincipal user, EasyScheduleDbContext db, CancellationToken ct) =>
        {
            if (!user.CanAccessSchool(schoolId)) return Results.Forbid();
            if (string.IsNullOrWhiteSpace(req.Name))
                return Results.Problem("ต้องระบุชื่อระดับชั้น", statusCode: StatusCodes.Status400BadRequest);
            if (req.Code is < 10 or > 99)
                return Results.Problem("รหัสระดับชั้นต้องอยู่ระหว่าง 10-99 (เช่น 11 = ป.1, 21 = ม.1, 34 = ม.4)", statusCode: StatusCodes.Status400BadRequest);
            if (req.StartPeriod < 1 || req.EndPeriod < req.StartPeriod)
                return Results.Problem("คาบเริ่ม-คาบสุดท้ายของระดับชั้นไม่ถูกต้อง", statusCode: StatusCodes.Status400BadRequest);

            var term = await ResolveTermAsync(db, schoolId, req.TermId, ct);
            if (term is null)
                return Results.Problem("ไม่พบปี/เทอมจัดตารางของโรงเรียนนี้", statusCode: StatusCodes.Status400BadRequest);

            if (await db.ClassLevels.AnyAsync(l => l.TermId == term.Value && l.Code == req.Code, ct))
                return Results.Problem($"รหัสระดับชั้น {req.Code} มีอยู่แล้วในเทอมนี้", statusCode: StatusCodes.Status409Conflict);

            var level = new ClassLevel
            {
                SchoolId = schoolId,
                TermId = term.Value,
                Code = req.Code,
                Name = req.Name.Trim(),
                StartPeriod = req.StartPeriod,
                EndPeriod = req.EndPeriod,
            };
            db.ClassLevels.Add(level);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/schools/{schoolId}/classlevels/{level.Id}",
                new ClassLevelDto(level.Id, level.Code, level.Name, level.StartPeriod, level.EndPeriod, []));
        }).RequireAuthorization(Policies.CanEdit);
    }

    private static async Task<int?> ResolveTermAsync(EasyScheduleDbContext db, int schoolId, int? termId, CancellationToken ct) =>
        termId is int id
            ? await db.Terms.Where(t => t.SchoolId == schoolId && t.Id == id).Select(t => (int?)t.Id).FirstOrDefaultAsync(ct)
            : await db.Terms.Where(t => t.SchoolId == schoolId && t.IsActive).OrderByDescending(t => t.Year).ThenByDescending(t => t.TermNo)
                .Select(t => (int?)t.Id).FirstOrDefaultAsync(ct);
}
