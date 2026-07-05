using MyProject.Core.Domain.Enums;
using MyProject.Core.Domain.Workflows;

namespace MyProject.Core.Domain.Approvals;

/// <summary>
/// Immutable snapshot of one workflow stage, captured at submit/resubmit time and materialized into a
/// <see cref="StageInstance"/>. Because instances run against this frozen copy, later edits to the
/// workflow template never affect in-flight approvals.
/// </summary>
public sealed record StageDefinition(
    int? WorkflowStageId,
    int StageOrder,
    string Name,
    ApproverType ApproverType,
    int? ApproverRoleId,
    int? ApproverDepartmentId,
    int? ApproverEmployeeId,
    int? SlaHours)
{
    public static StageDefinition From(WorkflowStage stage) => new(
        stage.Id,
        stage.StageOrder,
        stage.Name,
        stage.ApproverType,
        stage.ApproverRoleId,
        stage.ApproverDepartmentId,
        stage.ApproverEmployeeId,
        stage.SlaHours);
}

/// <summary>Result of an approve action: whether the flow advanced to a next stage or completed.</summary>
public enum ApprovalOutcome
{     Advanced = 1,
    Completed = 2
}
