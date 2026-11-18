namespace EasySchedule.Api.Endpoints;

public sealed record LoginRequest(string Username, string Password);
public sealed record UserDto(int Id, string Username, string DisplayName, string Role, int? SchoolId);
public sealed record LoginResponse(string Token, DateTime ExpiresAt, UserDto User);

public sealed record ClassSectionDto(int Id, int Number, string Name);
public sealed record ClassLevelDto(int Id, string Code, string Name, int SortOrder, IReadOnlyList<ClassSectionDto> Sections);
public sealed record CreateClassLevelRequest(string Code, string Name, int SortOrder, int? ScheduleModeId);

public sealed record HealthResponse(string Status, string Database, DateTime Time);
