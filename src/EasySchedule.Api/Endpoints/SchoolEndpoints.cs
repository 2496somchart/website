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

        g.MapGet("/classlevels", async (int schoolId, ClaimsPrincipal user, EasyScheduleDbContext db, CancellationToken ct) =>
        {
            if (!user.CanAccessSchool(schoolId)) return Results.Forbid();

            var levels = await db.ClassLevels
                .Where(l => l.SchoolId == schoolId)
                .OrderBy(l => l.SortOrder).ThenBy(l => l.Code)
                .Select(l => new ClassLevelDto(l.Id, l.Code, l.Name, l.SortOrder,
                    l.Sections.OrderBy(s => s.Number).Select(s => new ClassSectionDto(s.Id, s.Number, s.Name)).ToList()))
                .ToListAsync(ct);
            return Results.Ok(levels);
        }).RequireAuthorization(Policies.AnyUser);

        g.MapPost("/classlevels", async (int schoolId, CreateClassLevelRequest req, ClaimsPrincipal user, EasyScheduleDbContext db, CancellationToken ct) =>
        {
            if (!user.CanAccessSchool(schoolId)) return Results.Forbid();
            if (string.IsNullOrWhiteSpace(req.Code) || string.IsNullOrWhiteSpace(req.Name))
                return Results.Problem("ต้องระบุรหัสและชื่อระดับชั้น", statusCode: StatusCodes.Status400BadRequest);

            var modeId = req.ScheduleModeId
                         ?? await db.ScheduleModes.Where(m => m.SchoolId == schoolId).OrderBy(m => m.Id).Select(m => (int?)m.Id).FirstOrDefaultAsync(ct);
            if (modeId is null || !await db.ScheduleModes.AnyAsync(m => m.Id == modeId && m.SchoolId == schoolId, ct))
                return Results.Problem("ไม่พบรูปแบบตารางของโรงเรียนนี้", statusCode: StatusCodes.Status400BadRequest);

            if (await db.ClassLevels.AnyAsync(l => l.SchoolId == schoolId && l.Code == req.Code, ct))
                return Results.Problem("รหัสระดับชั้นนี้มีอยู่แล้ว", statusCode: StatusCodes.Status409Conflict);

            var level = new ClassLevel { SchoolId = schoolId, Code = req.Code.Trim(), Name = req.Name.Trim(), SortOrder = req.SortOrder, ScheduleModeId = modeId.Value };
            db.ClassLevels.Add(level);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/schools/{schoolId}/classlevels/{level.Id}",
                new ClassLevelDto(level.Id, level.Code, level.Name, level.SortOrder, []));
        }).RequireAuthorization(Policies.CanEdit);
    }
}
