using MyProject.Core.Application.Approvals;

namespace MyProject.Api.Endpoints;

/// <summary>The generic approval engine endpoints — the same routes serve every document type.</summary>
public static class ApprovalEndpoints
{
    public static void MapApprovalEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/approvals").WithTags("Approvals").RequireAuthorization();

        group.MapPost("/submit", async (SubmitApprovalRequest req, ApprovalAppService svc, CancellationToken ct) =>
            Results.Ok(await svc.SubmitAsync(req, ct)));

        group.MapGet("/{id:long}", async (long id, ApprovalAppService svc, CancellationToken ct) =>
            Results.Ok(await svc.GetAsync(id, ct)));

        group.MapGet("/{id:long}/history", async (long id, ApprovalAppService svc, CancellationToken ct) =>
            Results.Ok(await svc.GetHistoryAsync(id, ct)));

        group.MapPost("/{id:long}/approve", async (long id, ApproveRequest? body, ApprovalAppService svc, CancellationToken ct) =>
            Results.Ok(await svc.ApproveAsync(id, body ?? new ApproveRequest(null), ct)));

        group.MapPost("/{id:long}/reject", async (long id, RejectRequest body, ApprovalAppService svc, CancellationToken ct) =>
            Results.Ok(await svc.RejectAsync(id, body, ct)));

        group.MapPost("/{id:long}/comment", async (long id, CommentRequest body, ApprovalAppService svc, CancellationToken ct) =>
            Results.Ok(await svc.CommentAsync(id, body, ct)));

        group.MapPost("/{id:long}/resubmit", async (long id, ApprovalAppService svc, CancellationToken ct) =>
            Results.Ok(await svc.ResubmitAsync(id, ct)));

        group.MapPost("/{id:long}/cancel", async (long id, CancelRequest? body, ApprovalAppService svc, CancellationToken ct) =>
            Results.Ok(await svc.CancelAsync(id, body ?? new CancelRequest(null), ct)));
    }
}
