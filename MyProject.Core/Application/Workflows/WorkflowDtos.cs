using MyProject.Core.Domain.Workflows;

namespace MyProject.Core.Application.Workflows;

// ---- requests ----
public sealed record CreateDocumentTypeRequest(string Code, string Name);
public sealed record CreateWorkflowRequest(int DocumentTypeId, string Name);
public sealed record RenameWorkflowRequest(string Name);
public sealed record StageRequest(string ApproverType, string Name, int? RoleId, int? DepartmentId, int? EmployeeId, int? SlaHours);
public sealed record ReorderStagesRequest(IReadOnlyList<int> OrderedStageIds);

// ---- responses ----
public sealed record DocumentTypeDto(int Id, string Code, string Name, bool IsActive);

public sealed record WorkflowStageDto(
    int Id, int StageOrder, string Name, string ApproverType,
    int? ApproverRoleId, int? ApproverDepartmentId, int? ApproverEmployeeId, int? SlaHours);

public sealed record WorkflowDto(
    int Id, int DocumentTypeId, string Name, int Version, bool IsActive,
    IReadOnlyList<WorkflowStageDto> Stages);

public static class WorkflowMappings
{
    public static DocumentTypeDto ToDto(this DocumentType d) => new(d.Id, d.Code, d.Name, d.IsActive);

    public static WorkflowStageDto ToDto(this WorkflowStage s) => new(
        s.Id, s.StageOrder, s.Name, s.ApproverType.ToString(),
        s.ApproverRoleId, s.ApproverDepartmentId, s.ApproverEmployeeId, s.SlaHours);

    public static WorkflowDto ToDto(this Workflow w) => new(
        w.Id, w.DocumentTypeId, w.Name, w.Version, w.IsActive,
        w.OrderedStages.Select(ToDto).ToList());
}
