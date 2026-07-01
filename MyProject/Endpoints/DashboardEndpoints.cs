using MyProject.Core.Application.Dashboards;

namespace MyProject.Api.Endpoints;

public static class DashboardEndpoints
{
    public static void MapDashboardEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/dashboard").WithTags("Dashboard").RequireAuthorization();

        group.MapGet("/my-pending", (DashboardQueryService svc, CancellationToken ct) => svc.MyPendingAsync(ct));
        group.MapGet("/my-documents", (DashboardQueryService svc, CancellationToken ct) => svc.MyDocumentsAsync(ct));
        group.MapGet("/sla-breaches", (DashboardQueryService svc, CancellationToken ct) => svc.SlaBreachesAsync(ct));
        group.MapGet("/workflows/{workflowId:int}/metrics", async (int workflowId, DashboardQueryService svc, CancellationToken ct) =>
            Results.Ok(await svc.WorkflowMetricsAsync(workflowId, ct)));
    }
}
