using MyProject.Api.Auth;
using MyProject.Core.Application.Workflows;

namespace MyProject.Api.Endpoints;

/// <summary>Administrative endpoints for document types and workflow templates (WorkflowAdmin only).</summary>
public static class WorkflowAdminEndpoints
{
    public static void MapWorkflowAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var docTypes = app.MapGroup("/api/document-types").WithTags("Document Types")
            .RequireAuthorization(Policies.WorkflowAdmin);

        docTypes.MapGet("/", (WorkflowAdminService svc, CancellationToken ct) => svc.ListDocumentTypesAsync(ct));
        docTypes.MapPost("/", async (CreateDocumentTypeRequest req, WorkflowAdminService svc, CancellationToken ct) =>
            Results.Ok(await svc.CreateDocumentTypeAsync(req, ct)));

        var workflows = app.MapGroup("/api/workflows").WithTags("Workflows")
            .RequireAuthorization(Policies.WorkflowAdmin);

        workflows.MapGet("/", (int? documentTypeId, WorkflowAdminService svc, CancellationToken ct) =>
            svc.ListWorkflowsAsync(documentTypeId, ct));
        workflows.MapGet("/{id:int}", async (int id, WorkflowAdminService svc, CancellationToken ct) =>
            Results.Ok(await svc.GetWorkflowAsync(id, ct)));
        workflows.MapPost("/", async (CreateWorkflowRequest req, WorkflowAdminService svc, CancellationToken ct) =>
            Results.Ok(await svc.CreateWorkflowAsync(req, ct)));
        workflows.MapPut("/{id:int}", async (int id, RenameWorkflowRequest req, WorkflowAdminService svc, CancellationToken ct) =>
            Results.Ok(await svc.RenameWorkflowAsync(id, req, ct)));

        workflows.MapPost("/{id:int}/stages", async (int id, StageRequest req, WorkflowAdminService svc, CancellationToken ct) =>
            Results.Ok(await svc.AddStageAsync(id, req, ct)));
        workflows.MapPut("/{id:int}/stages/{stageId:int}", async (int id, int stageId, StageRequest req, WorkflowAdminService svc, CancellationToken ct) =>
            Results.Ok(await svc.UpdateStageAsync(id, stageId, req, ct)));
        workflows.MapDelete("/{id:int}/stages/{stageId:int}", async (int id, int stageId, WorkflowAdminService svc, CancellationToken ct) =>
            Results.Ok(await svc.RemoveStageAsync(id, stageId, ct)));
        workflows.MapPut("/{id:int}/stages/reorder", async (int id, ReorderStagesRequest req, WorkflowAdminService svc, CancellationToken ct) =>
            Results.Ok(await svc.ReorderStagesAsync(id, req, ct)));

        workflows.MapPost("/{id:int}/activate", async (int id, WorkflowAdminService svc, CancellationToken ct) =>
            Results.Ok(await svc.ActivateWorkflowAsync(id, ct)));
    }
}
