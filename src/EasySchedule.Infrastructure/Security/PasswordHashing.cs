using EasySchedule.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace EasySchedule.Infrastructure.Security;

public interface IPasswordService
{
    string Hash(User user, string password);
    bool Verify(User user, string password);
}

/// <summary>ใช้ PBKDF2 ของ ASP.NET Core Identity</summary>
public sealed class PasswordService : IPasswordService
{
    private readonly PasswordHasher<User> _hasher = new();

    public string Hash(User user, string password) => _hasher.HashPassword(user, password);

    public bool Verify(User user, string password) =>
        _hasher.VerifyHashedPassword(user, user.PasswordHash, password) != PasswordVerificationResult.Failed;
}
