using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using MyProject.Api.Auth;
using MyProject.Core.Application.Abstractions;
using MyProject.Core.Application.Auth;
using MyProject.Core.Infrastructure.Persistence;

namespace MyProject.Api.Endpoints;

/// <summary>Password-based authentication: registration, login, password change, and admin provisioning.</summary>
public static class AuthEndpoints
{
    public sealed record AuthResponse(string Token, int EmployeeId, string FullName, IReadOnlyCollection<string> Roles);

    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        // ---- self-service sign-up ----
        group.MapPost("/register", async (
            RegisterRequest req, HttpContext http, AuthAppService svc, IJwtTokenService jwt, CancellationToken ct) =>
        {
            var result = await svc.RegisterAsync(req, Ip(http), ct);
            return Results.Ok(ToResponse(result, jwt));
        }).AllowAnonymous();

        // ---- login ----
        group.MapPost("/login", async (
            LoginRequest req, HttpContext http, AuthAppService svc, IJwtTokenService jwt, CancellationToken ct) =>
        {
            var result = await svc.LoginAsync(req, Ip(http), ct);
            return result is null ? Results.Unauthorized() : Results.Ok(ToResponse(result, jwt));
        }).AllowAnonymous();

        // ---- change own password ----
        group.MapPost("/change-password", async (
            ChangePasswordRequest req, HttpContext http, AuthAppService svc, ICurrentUser currentUser, CancellationToken ct) =>
        {
            await svc.ChangePasswordAsync(currentUser.EmployeeId, req, Ip(http), ct);
            return Results.NoContent();
        }).RequireAuthorization();

        // ---- admin provisions an account ----
        group.MapPost("/admin/users", async (
            AdminCreateUserRequest req, HttpContext http, AuthAppService svc, ICurrentUser currentUser, CancellationToken ct) =>
            Results.Ok(await svc.AdminCreateUserAsync(req, currentUser.EmployeeId, Ip(http), ct)))
            .RequireAuthorization(Policies.WorkflowAdmin);

        // ---- departments lookup for the sign-up form (pre-auth) ----
        group.MapGet("/departments", async (AppDbContext db, CancellationToken ct) =>
            await db.Departments.OrderBy(d => d.Name)
                .Select(d => new { d.Id, d.Name }).ToListAsync(ct))
            .AllowAnonymous();

        // Diagnostic: shows the authenticated principal exactly as the API sees it.
        group.MapGet("/whoami", (ClaimsPrincipal user) => Results.Ok(new
        {
            isAuthenticated = user.Identity?.IsAuthenticated ?? false,
            roleClaimType = (user.Identity as ClaimsIdentity)?.RoleClaimType,
            isWorkflowAdmin = user.IsInRole("WorkflowAdmin"),
            claims = user.Claims.Select(c => new { c.Type, c.Value })
        })).RequireAuthorization();
    }

    private static AuthResponse ToResponse(AuthResult result, IJwtTokenService jwt)
    {
        var token = jwt.CreateToken(result.Employee, result.Roles);
        return new AuthResponse(token, result.Employee.Id, result.Employee.FullName, result.Roles);
    }

    private static string? Ip(HttpContext http) => http.Connection.RemoteIpAddress?.ToString();
}
