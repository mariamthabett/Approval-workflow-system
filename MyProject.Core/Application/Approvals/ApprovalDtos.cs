using MyProject.Core.Domain.Approvals;

namespace MyProject.Core.Application.Approvals;

// ---- requests ----
public sealed record SubmitApprovalRequest(string DocumentTypeCode, string DocumentId);
public sealed record ApproveRequest(string? Comment);
public sealed record RejectRequest(string Comment);
public sealed record CommentRequest(string Comment);
public sealed record CancelRequest(string? Comment);

// ---- responses ----
public sealed record StageInstanceDto(
    long Id, int CycleNumber, int StageOrder, string Name, string ApproverType,
    string Status, int? ActedByEmployeeId, DateTime? ActedAtUtc, DateTime EnteredAtUtc, DateTime? DueAtUtc);

public sealed record ApprovalInstanceDto(
    long Id, int DocumentTypeId, string DocumentId, int WorkflowId, int WorkflowVersion,
    string Status, int CycleNumber, int? CurrentStageOrder, string? CurrentStageName,
    int InitiatorEmployeeId, DateTime CreatedAtUtc, DateTime? CompletedAtUtc,
    IReadOnlyList<StageInstanceDto> Stages);

public sealed record ApprovalActionDto(
    long Id, int CycleNumber, string ActionType, int ActedByEmployeeId, string? Comment,
    string? FromStatus, string? ToStatus, DateTime CreatedAtUtc);

public static class ApprovalMappings
{
    public static StageInstanceDto ToDto(this StageInstance s) => new(
        s.Id, s.CycleNumber, s.StageOrder, s.Name, s.ApproverType.ToString(),
        s.Status.ToString(), s.ActedByEmployeeId, s.ActedAtUtc, s.EnteredAtUtc, s.DueAtUtc);

    public static ApprovalInstanceDto ToDto(this ApprovalInstance i) => new(
        i.Id, i.DocumentTypeId, i.DocumentId, i.WorkflowId, i.WorkflowVersion,
        i.Status.ToString(), i.CycleNumber, i.CurrentStageOrder, i.CurrentStage?.Name,
        i.InitiatorEmployeeId, i.CreatedAtUtc, i.CompletedAtUtc,
        i.OrderedStages.Select(ToDto).ToList());

    public static ApprovalActionDto ToDto(this ApprovalAction a) => new(
        a.Id, a.CycleNumber, a.ActionType.ToString(), a.ActedByEmployeeId, a.Comment,
        a.FromStatus?.ToString(), a.ToStatus?.ToString(), a.CreatedAtUtc);
}
