using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MyProject.Api.Auth;
using MyProject.Api.Endpoints;
using MyProject.Api.ExceptionHandling;
using MyProject.Api.Outbox;
using MyProject.Core.Application;
using MyProject.Core.Application.Abstractions;
using MyProject.Core.Infrastructure;
using MyProject.Core.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// ---- configuration ----
// Provider: "SqlServer" (production default) or "Sqlite" (zero-install dev/verification).
var dbProvider = builder.Configuration["Database:Provider"] ?? "SqlServer";
var isSqlite = string.Equals(dbProvider, "Sqlite", StringComparison.OrdinalIgnoreCase);
var connectionString = (isSqlite
        ? builder.Configuration.GetConnectionString("Sqlite")
        : builder.Configuration.GetConnectionString("Default"))
    ?? throw new InvalidOperationException($"Missing connection string for provider '{dbProvider}'.");
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();

// ---- services (composition root) ----
builder.Services.AddSingleton(jwtSettings);
builder.Services.AddInfrastructure(dbProvider, connectionString);   // DbContext, repos, resolver, outbox handlers
builder.Services.AddApplication();                       // use-case services

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddHostedService<OutboxDispatcher>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Keep our short claim names ("role", "employeeId") verbatim instead of remapping them to the
        // long WS-* URIs — so RoleClaimType/NameClaimType below match the actual token claims.
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SigningKey)),
            ValidateLifetime = true,
            RoleClaimType = AppClaims.Role,
            NameClaimType = AppClaims.EmployeeId
        };
    });

builder.Services.AddAuthorization(options =>
    options.AddPolicy(Policies.WorkflowAdmin, policy => policy.RequireRole("WorkflowAdmin")));

builder.Services.AddOpenApi();

var app = builder.Build();

// ---- database: apply migrations and seed demo data (convenience for local/dev) ----
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // SQLite dev: build the schema straight from the model (no migrations needed).
    // SQL Server: apply the versioned migrations.
    if (isSqlite)
        await db.Database.EnsureCreatedAsync();
    else
        await db.Database.MigrateAsync();
    await DataSeeder.SeedAsync(db);
}

// ---- pipeline ----
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapWorkflowAdminEndpoints();
app.MapApprovalEndpoints();
app.MapDashboardEndpoints();
app.MapLeaveRequestEndpoints();

app.Run();
