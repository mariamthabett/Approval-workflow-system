using MyProject.Api.Auth;
using MyProject.Core.Application.Abstractions;
using MyProject.Core.Application.Auditing;
using MyProject.Core.Domain.Enums;

namespace MyProject.Api.Endpoints;

/// <summary>Read access to the per-user activity log: own feed (any user) and the full feed (admins).</summary>
public static class ActivityEndpoints
{
    public static void MapActivityEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/activity").WithTags("Activity").RequireAuthorization();

        // The signed-in user's own activity.
        group.MapGet("/me", async (int? take, ActivityQueryService svc, ICurrentUser currentUser, CancellationToken ct) =>
            Results.Ok(await svc.ForEmployeeAsync(currentUser.EmployeeId, take ?? 100, ct)));

        // Full feed with optional filters — admins only.
        group.MapGet("/", async (
            int? employeeId, ActivityType? type, int? skip, int? take, ActivityQueryService svc, CancellationToken ct) =>
            Results.Ok(await svc.ListAsync(employeeId, type, skip ?? 0, take ?? 100, ct)))
            .RequireAuthorization(Policies.WorkflowAdmin);
    }
}
