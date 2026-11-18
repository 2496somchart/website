using System.Text;
using System.Text.Json.Serialization;
using EasySchedule.Api.Auth;
using EasySchedule.Api.Endpoints;
using EasySchedule.Infrastructure;
using EasySchedule.Infrastructure.Persistence;
using EasySchedule.Infrastructure.Tenancy;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.Services.AddInfrastructure(config);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantContext, HttpTenantContext>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<TokenService>();
builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// ---- JWT ----
var jwt = config.GetSection(JwtOptions.Section).Get<JwtOptions>() ?? new JwtOptions();
if (jwt.SigningKey.Length < 32)
    throw new InvalidOperationException("Jwt:SigningKey must be set (env Jwt__SigningKey) and be at least 32 characters.");
builder.Services.Configure<JwtOptions>(config.GetSection(JwtOptions.Section));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.MapInboundClaims = false;
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
            RoleClaimType = AppClaims.Role,
            NameClaimType = "unique_name",
            ClockSkew = TimeSpan.FromMinutes(1),
        };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(Policies.AnyUser, p => p.RequireRole(Roles.SuperAdmin, Roles.SchoolAdmin, Roles.Viewer))
    .AddPolicy(Policies.CanEdit, p => p.RequireRole(Roles.SuperAdmin, Roles.SchoolAdmin))
    .AddPolicy(Policies.SuperAdminOnly, p => p.RequireRole(Roles.SuperAdmin));

// ---- Swagger ----
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(o =>
{
    o.SwaggerDoc("v1", new OpenApiInfo { Title = "EasySchedule API", Version = "v1" });
    var scheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" },
    };
    o.AddSecurityDefinition("Bearer", scheme);
    o.AddSecurityRequirement(new OpenApiSecurityRequirement { [scheme] = [] });
});

var app = builder.Build();

await app.Services.InitializeDatabaseAsync(migrate: config.GetValue("Database:MigrateOnStartup", false));

if (app.Environment.IsDevelopment() || config.GetValue("Swagger:Enabled", false))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/api/health", async (EasyScheduleDbContext db, TimeProvider clock, CancellationToken ct) =>
{
    var dbOk = await db.Database.CanConnectAsync(ct);
    var body = new HealthResponse(dbOk ? "ok" : "degraded", dbOk ? "ok" : "unreachable", clock.GetUtcNow().UtcDateTime);
    return dbOk ? Results.Ok(body) : Results.Json(body, statusCode: StatusCodes.Status503ServiceUnavailable);
}).AllowAnonymous().WithTags("Health");

app.MapAuthEndpoints();
app.MapSchoolEndpoints();

app.Run();

public partial class Program;
