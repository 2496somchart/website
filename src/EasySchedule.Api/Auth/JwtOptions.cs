namespace EasySchedule.Api.Auth;

public sealed class JwtOptions
{
    public const string Section = "Jwt";
    public string Issuer { get; set; } = "EasySchedule";
    public string Audience { get; set; } = "EasySchedule";
    /// <summary>คีย์ลงนาม (≥ 32 ตัวอักษร) — มาจาก env Jwt__SigningKey เท่านั้น</summary>
    public string SigningKey { get; set; } = "";
    public int ExpiryMinutes { get; set; } = 480;
}
