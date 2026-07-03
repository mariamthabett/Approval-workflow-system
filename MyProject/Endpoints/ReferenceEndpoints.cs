using Microsoft.EntityFrameworkCore;
using MyProject.Core.Infrastructure.Persistence;

namespace MyProject.Api.Endpoints;

/// <summary>
/// Read-only reference lookups (employees, roles, departments) used to populate the admin UI pickers
/// when building workflow stages. Authenticated read access only — no writes.
/// </summary>
public static class ReferenceEndpoints
{
    public static void MapReferenceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/reference").WithTags("Reference").RequireAuthorization();

        group.MapGet("/employees", async (AppDbContext db, CancellationToken ct) =>
            await db.Employees.Where(e => e.IsActive).OrderBy(e => e.FullName)
                .Select(e => new { e.Id, e.FullName, e.Email, e.DepartmentId }).ToListAsync(ct));

        group.MapGet("/roles", async (AppDbContext db, CancellationToken ct) =>
            await db.Roles.OrderBy(r => r.Name)
                .Select(r => new { r.Id, r.Code, r.Name }).ToListAsync(ct));

        group.MapGet("/departments", async (AppDbContext db, CancellationToken ct) =>
            await db.Departments.OrderBy(d => d.Name)
                .Select(d => new { d.Id, d.Name, d.ManagerEmployeeId }).ToListAsync(ct));
    }
}
