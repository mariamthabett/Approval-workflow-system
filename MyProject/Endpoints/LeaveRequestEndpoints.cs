using MyProject.Core.Application.Documents;

namespace MyProject.Api.Endpoints;

/// <summary>Sample document module. Owns the LeaveRequest; delegates approval to the generic engine.</summary>
public static class LeaveRequestEndpoints
{
    public static void MapLeaveRequestEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/leave-requests").WithTags("Leave Requests").RequireAuthorization();

        group.MapPost("/", async (CreateLeaveRequest req, LeaveRequestService svc, CancellationToken ct) =>
        {
            var created = await svc.CreateAsync(req, ct);
            return Results.Created($"/api/leave-requests/{created.Id}", created);
        });

        group.MapGet("/{id:int}", async (int id, LeaveRequestService svc, CancellationToken ct) =>
            Results.Ok(await svc.GetAsync(id, ct)));

        group.MapPut("/{id:int}", async (int id, UpdateLeaveRequest req, LeaveRequestService svc, CancellationToken ct) =>
            Results.Ok(await svc.UpdateAsync(id, req, ct)));

        group.MapPost("/{id:int}/submit", async (int id, LeaveRequestService svc, CancellationToken ct) =>
            Results.Ok(await svc.SubmitAsync(id, ct)));
    }
}
