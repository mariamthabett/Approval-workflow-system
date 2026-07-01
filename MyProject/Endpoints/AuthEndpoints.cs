using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using MyProject.Core.Application.Abstractions;
using MyProject.Core.Infrastructure.Persistence;

namespace MyProject.Api.Endpoints;

/// <summary>Dev login stub: exchanges an employee email for a signed JWT. Replace with real auth / SSO.</summary>
public static class AuthEndpoints
{
    public sealed record LoginRequest(string Email);
    public sealed record LoginResponse(string Token, int EmployeeId, IReadOnlyCollection<string> Roles);

    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/login", async (LoginRequest req, AppDbContext db, IJwtTokenService jwt, CancellationToken ct) =>
        {
            var employee = await db.Employees.FirstOrDefaultAsync(e => e.Email == req.Email && e.IsActive, ct);
            if (employee is null) return Results.Unauthorized();

            var roles = await db.EmployeeRoles
                .Where(er => er.EmployeeId == employee.Id)
                .Join(db.Roles, er => er.RoleId, r => r.Id, (er, r) => r.Code)
                .ToListAsync(ct);

            var token = jwt.CreateToken(employee, roles);
            return Results.Ok(new LoginResponse(token, employee.Id, roles));
        }).AllowAnonymous();

        // Diagnostic: shows the authenticated principal exactly as the API sees it.
        group.MapGet("/whoami", (ClaimsPrincipal user) => Results.Ok(new
        {
            isAuthenticated = user.Identity?.IsAuthenticated ?? false,
            roleClaimType = (user.Identity as ClaimsIdentity)?.RoleClaimType,
            isWorkflowAdmin = user.IsInRole("WorkflowAdmin"),
            claims = user.Claims.Select(c => new { c.Type, c.Value })
        })).RequireAuthorization();
    }
}
