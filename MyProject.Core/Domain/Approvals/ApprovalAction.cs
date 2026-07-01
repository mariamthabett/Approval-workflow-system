using MyProject.Core.Domain.Enums;

namespace MyProject.Core.Domain.Approvals;

/// <summary>
/// A single immutable entry in the approval history (req 9). Append-only: once written it is never
/// updated or deleted (enforced by an EF interceptor and by database permissions). Records who did
/// what, when, on which stage/cycle, and the status transition.
/// </summary>
public sealed class ApprovalAction
{
    public long Id { get; private set; }
    public long ApprovalInstanceId { get; private set; }
    public long? StageInstanceId { get; private set; }
    public int CycleNumber { get; private set; }
    public ActionType ActionType { get; private set; }
    public int ActedByEmployeeId { get; private set; }
    public string? Comment { get; private set; }
    public InstanceStatus? FromStatus { get; private set; }
    public InstanceStatus? ToStatus { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private ApprovalAction() { }

    internal ApprovalAction(
        long? stageInstanceId,
        int cycleNumber,
        ActionType actionType,
        int actedByEmployeeId,
        string? comment,
        InstanceStatus? fromStatus,
        InstanceStatus? toStatus,
        DateTime createdAtUtc)
    {
        StageInstanceId = stageInstanceId;
        CycleNumber = cycleNumber;
        ActionType = actionType;
        ActedByEmployeeId = actedByEmployeeId;
        Comment = comment;
        FromStatus = fromStatus;
        ToStatus = toStatus;
        CreatedAtUtc = createdAtUtc;
    }
}
