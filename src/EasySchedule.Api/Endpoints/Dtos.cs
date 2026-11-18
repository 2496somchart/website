namespace EasySchedule.Api.Endpoints;

public sealed record LoginRequest(string Username, string Password);
public sealed record UserDto(int Id, string Username, string DisplayName, string Role, int? SchoolId);
public sealed record LoginResponse(string Token, DateTime ExpiresAt, UserDto User);

public sealed record ClassSectionDto(int Id, int SectionNo, string Name);
public sealed record ClassLevelDto(int Id, int Code, string Name, int StartPeriod, int EndPeriod, IReadOnlyList<ClassSectionDto> Sections);
public sealed record CreateClassLevelRequest(int Code, string Name, int StartPeriod, int EndPeriod, int? TermId);

public sealed record HealthResponse(string Status, string Database, DateTime Time);
