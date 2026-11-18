using System.Security.Claims;
using EasySchedule.Api.Auth;
using EasySchedule.Infrastructure.Persistence;
using EasySchedule.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace EasySchedule.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var g = app.MapGroup("/api/auth").WithTags("Auth");

        g.MapPost("/login", async (LoginRequest req, EasyScheduleDbContext db, IPasswordService passwords, TokenService tokens, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrEmpty(req.Password))
                return Results.Problem("กรุณากรอกชื่อผู้ใช้และรหัสผ่าน", statusCode: StatusCodes.Status400BadRequest);

            var user = await db.Users.FirstOrDefaultAsync(u => u.Username == req.Username.Trim() && u.IsActive, ct);
            if (user is null || !passwords.Verify(user, req.Password))
                return Results.Problem("ชื่อผู้ใช้หรือรหัสผ่านไม่ถูกต้อง", statusCode: StatusCodes.Status401Unauthorized);

            var (token, expires) = tokens.Issue(user);
            return Results.Ok(new LoginResponse(token, expires,
                new UserDto(user.Id, user.Username, user.DisplayName, user.Role.ToString(), user.SchoolId)));
        }).AllowAnonymous();

        g.MapGet("/me", async (ClaimsPrincipal principal, EasyScheduleDbContext db, CancellationToken ct) =>
        {
            if (!int.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal.FindFirstValue("sub"), out var id))
                return Results.Unauthorized();
            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
            return user is null
                ? Results.Unauthorized()
                : Results.Ok(new UserDto(user.Id, user.Username, user.DisplayName, user.Role.ToString(), user.SchoolId));
        }).RequireAuthorization(Policies.AnyUser);
    }
}
