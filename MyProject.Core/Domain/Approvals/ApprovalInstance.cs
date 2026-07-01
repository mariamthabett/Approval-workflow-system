using MyProject.Core.Domain.Common;
using MyProject.Core.Domain.Enums;

namespace MyProject.Core.Domain.Approvals;

/// <summary>
/// Runtime aggregate root: one per (DocumentType, DocumentId). Owns the approval state machine and the
/// immutable action log. All transitions flow through here so the invariants (no stage skipped,
/// sequential advance, resubmit starts a fresh cycle while preserving history) live in one place.
///
/// The "current approver" authorization check is performed by the application layer (it needs role /
/// department membership from the database); this aggregate enforces *which stage* may transition and
/// *what* the resulting state is, and records *who* acted.
/// </summary>
public sealed class ApprovalInstance : IAggregateRoot
{
    private readonly List<StageInstance> _stages = new();
    private readonly List<ApprovalAction> _actions = new();

    public long Id { get; private set; }
    public int DocumentTypeId { get; private set; }
    public string DocumentId { get; private set; } = string.Empty;
    public int WorkflowId { get; private set; }
    public int WorkflowVersion { get; private set; }
    public InstanceStatus Status { get; private set; }
    public int CycleNumber { get; private set; }
    public int? CurrentStageOrder { get; private set; }
    public int InitiatorEmployeeId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }

    /// <summary>Optimistic-concurrency token — blocks lost updates when two eligible approvers race.</summary>
    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    // Backing-collection navigations (EF uses the fields). Order at call sites via the helpers below.
    public IReadOnlyCollection<StageInstance> Stages => _stages.AsReadOnly();
    public IReadOnlyCollection<ApprovalAction> Actions => _actions.AsReadOnly();

    public IReadOnlyList<StageInstance> OrderedStages =>
        _stages.OrderBy(s => s.CycleNumber).ThenBy(s => s.StageOrder).ToList();

    public IReadOnlyList<ApprovalAction> OrderedActions =>
        _actions.OrderBy(a => a.CreatedAtUtc).ThenBy(a => a.Id).ToList();

    /// <summary>The single stage awaiting action in the current cycle, or null if terminal.</summary>
    public StageInstance? CurrentStage => CurrentStageOrder is null
        ? null
        : _stages.SingleOrDefault(s => s.CycleNumber == CycleNumber && s.StageOrder == CurrentStageOrder.Value);

    private ApprovalInstance() { }

    /// <summary>Begin approval for a freshly submitted document (cycle 1).</summary>
    public static ApprovalInstance Start(
        int documentTypeId,
        string documentId,
        int workflowId,
        int workflowVersion,
        IReadOnlyList<StageDefinition> stages,
        int initiatorEmployeeId,
        DateTime nowUtc)
    {
        ValidateStages(stages);
        var instance = new ApprovalInstance
        {
            DocumentTypeId = documentTypeId,
            DocumentId = documentId,
            WorkflowId = workflowId,
            WorkflowVersion = workflowVersion,
            Status = InstanceStatus.Pending,
            CycleNumber = 1,
            CurrentStageOrder = 1,
            InitiatorEmployeeId = initiatorEmployeeId,
            CreatedAtUtc = nowUtc
        };
        instance.MaterializeCycle(stages, nowUtc);
        instance.Record(null, ActionType.Submit, initiatorEmployeeId, null, null, InstanceStatus.Pending, nowUtc);
        return instance;
    }

    /// <summary>Approve the current stage; advance to the next stage or complete the instance.</summary>
    public ApprovalOutcome Approve(int actingEmployeeId, string? comment, DateTime nowUtc)
    {
        var stage = RequireActionableStage();
        stage.Approve(actingEmployeeId, nowUtc);

        var nextOrder = stage.StageOrder + 1;
        var hasNext = _stages.Any(s => s.CycleNumber == CycleNumber && s.StageOrder == nextOrder);
        if (hasNext)
        {
            CurrentStageOrder = nextOrder;
            Record(stage.Id, ActionType.Approve, actingEmployeeId, comment, InstanceStatus.Pending, InstanceStatus.Pending, nowUtc);
            return ApprovalOutcome.Advanced;
        }

        Status = InstanceStatus.Approved;
        CurrentStageOrder = null;
        CompletedAtUtc = nowUtc;
        Record(stage.Id, ActionType.Approve, actingEmployeeId, comment, InstanceStatus.Pending, InstanceStatus.Approved, nowUtc);
        return ApprovalOutcome.Completed;
    }

    /// <summary>Reject the current stage; the whole instance becomes Rejected (owner may resubmit).</summary>
    public void Reject(int actingEmployeeId, string comment, DateTime nowUtc)
    {
        if (string.IsNullOrWhiteSpace(comment))
            throw new DomainException("A rejection requires a comment explaining the reason.");
        var stage = RequireActionableStage();
        stage.Reject(actingEmployeeId, nowUtc);
        Status = InstanceStatus.Rejected;
        CurrentStageOrder = null;
        Record(stage.Id, ActionType.Reject, actingEmployeeId, comment, InstanceStatus.Pending, InstanceStatus.Rejected, nowUtc);
    }

    /// <summary>
    /// Start a new approval cycle after a rejection, using a fresh snapshot of the (possibly updated)
    /// active workflow. Prior cycles' stage instances and all actions remain untouched — full history
    /// is preserved (req 8, 9).
    /// </summary>
    public void Resubmit(IReadOnlyList<StageDefinition> stages, int actingEmployeeId, DateTime nowUtc)
    {
        if (Status != InstanceStatus.Rejected)
            throw new DomainException("Only a rejected document can be resubmitted.");
        ValidateStages(stages);

        CycleNumber++;
        MaterializeCycle(stages, nowUtc);
        Status = InstanceStatus.Pending;
        CurrentStageOrder = 1;
        CompletedAtUtc = null;
        Record(null, ActionType.Resubmit, actingEmployeeId, null, InstanceStatus.Rejected, InstanceStatus.Pending, nowUtc);
    }

    /// <summary>Add a comment to an active instance without changing its state.</summary>
    public void AddComment(int actingEmployeeId, string comment, DateTime nowUtc)
    {
        if (Status != InstanceStatus.Pending)
            throw new DomainException("Comments can only be added while the approval is in progress.");
        if (string.IsNullOrWhiteSpace(comment))
            throw new DomainException("Comment text is required.");
        Record(CurrentStage?.Id, ActionType.Comment, actingEmployeeId, comment, null, null, nowUtc);
    }

    /// <summary>Cancel an in-progress approval (owner/admin).</summary>
    public void Cancel(int actingEmployeeId, string? comment, DateTime nowUtc)
    {
        if (Status != InstanceStatus.Pending)
            throw new DomainException("Only an in-progress approval can be cancelled.");
        Status = InstanceStatus.Cancelled;
        CurrentStageOrder = null;
        CompletedAtUtc = nowUtc;
        Record(null, ActionType.Cancel, actingEmployeeId, comment, InstanceStatus.Pending, InstanceStatus.Cancelled, nowUtc);
    }

    private StageInstance RequireActionableStage()
    {
        if (Status != InstanceStatus.Pending)
            throw new DomainException($"The approval is not in progress (status: {Status}).");
        return CurrentStage
            ?? throw new DomainException("There is no current stage to act on.");
    }

    private void MaterializeCycle(IReadOnlyList<StageDefinition> stages, DateTime nowUtc)
    {
        foreach (var def in stages.OrderBy(s => s.StageOrder))
            _stages.Add(new StageInstance(CycleNumber, def, nowUtc));
    }

    private void Record(long? stageInstanceId, ActionType type, int actor, string? comment,
        InstanceStatus? from, InstanceStatus? to, DateTime nowUtc)
        => _actions.Add(new ApprovalAction(stageInstanceId, CycleNumber, type, actor, comment, from, to, nowUtc));

    private static void ValidateStages(IReadOnlyList<StageDefinition> stages)
    {
        if (stages.Count == 0)
            throw new DomainException("Cannot start approval: the workflow has no stages.");
        var orders = stages.Select(s => s.StageOrder).OrderBy(o => o).ToList();
        for (var i = 0; i < orders.Count; i++)
            if (orders[i] != i + 1)
                throw new DomainException("Workflow stage orders must be contiguous starting at 1.");
    }
}
