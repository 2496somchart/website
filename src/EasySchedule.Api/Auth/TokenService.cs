using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EasySchedule.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EasySchedule.Api.Auth;

public sealed class TokenService(IOptions<JwtOptions> options, TimeProvider clock)
{
    private readonly JwtOptions _o = options.Value;

    public (string Token, DateTime ExpiresAt) Issue(User user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.Username),
            new(AppClaims.Role, user.Role.ToString()),
            new("name", user.DisplayName),
        };
        if (user.SchoolId is int sid)
            claims.Add(new Claim(AppClaims.SchoolId, sid.ToString()));

        var expires = clock.GetUtcNow().UtcDateTime.AddMinutes(_o.ExpiryMinutes);
        var creds = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_o.SigningKey)), SecurityAlgorithms.HmacSha256);
        var jwt = new JwtSecurityToken(_o.Issuer, _o.Audience, claims, expires: expires, signingCredentials: creds);
        return (new JwtSecurityTokenHandler().WriteToken(jwt), expires);
    }
}
