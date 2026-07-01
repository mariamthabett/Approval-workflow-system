using MyProject.Core.Domain.Common;
using MyProject.Core.Domain.Enums;

namespace MyProject.Core.Domain.Approvals;

/// <summary>
/// A stage as it exists inside a running approval cycle — a frozen copy of a workflow stage plus
/// runtime status. Child of the <see cref="ApprovalInstance"/> aggregate.
/// </summary>
public sealed class StageInstance
{
    public long Id { get; private set; }
    public long ApprovalInstanceId { get; private set; }
    public int? WorkflowStageId { get; private set; }
    public int CycleNumber { get; private set; }
    public int StageOrder { get; private set; }
    public string Name { get; private set; } = string.Empty;

    public ApproverType ApproverType { get; private set; }
    public int? ResolvedApproverEmployeeId { get; private set; }
    public int? ApproverRoleId { get; private set; }
    public int? ApproverDepartmentId { get; private set; }

    public StageStatus Status { get; private set; } = StageStatus.Pending;
    public int? ActedByEmployeeId { get; private set; }
    public DateTime? ActedAtUtc { get; private set; }
    public DateTime EnteredAtUtc { get; private set; }
    public int? SlaHours { get; private set; }

    private StageInstance() { }

    internal StageInstance(int cycleNumber, StageDefinition def, DateTime enteredAtUtc)
    {
        CycleNumber = cycleNumber;
        WorkflowStageId = def.WorkflowStageId;
        StageOrder = def.StageOrder;
        Name = def.Name;
        ApproverType = def.ApproverType;
        ApproverRoleId = def.ApproverRoleId;
        ApproverDepartmentId = def.ApproverDepartmentId;
        // For a User stage the approver is fixed at snapshot time; Role/Department stay unresolved
        // (any eligible member may act) and are resolved dynamically at authorization/notification time.
        ResolvedApproverEmployeeId = def.ApproverType == ApproverType.User ? def.ApproverEmployeeId : null;
        SlaHours = def.SlaHours;
        EnteredAtUtc = enteredAtUtc;
    }

    /// <summary>UTC deadline for this stage, if an SLA is configured.</summary>
    public DateTime? DueAtUtc => SlaHours.HasValue ? EnteredAtUtc.AddHours(SlaHours.Value) : null;

    internal void Approve(int actedByEmployeeId, DateTime whenUtc)
    {
        Guard();
        Status = StageStatus.Approved;
        ActedByEmployeeId = actedByEmployeeId;
        ActedAtUtc = whenUtc;
    }

    internal void Reject(int actedByEmployeeId, DateTime whenUtc)
    {
        Guard();
        Status = StageStatus.Rejected;
        ActedByEmployeeId = actedByEmployeeId;
        ActedAtUtc = whenUtc;
    }

    private void Guard()
    {
        if (Status != StageStatus.Pending)
            throw new DomainException($"Stage '{Name}' has already been actioned ({Status}).");
    }
}
