using MyProject.Core.Domain.Common;

namespace MyProject.Core.Domain.Workflows;

/// <summary>
/// Admin-authored approval template for a document type. Aggregate root over its ordered stages.
/// Invariants: stage orders are contiguous 1..N; a workflow needs at least one stage to be activated.
/// Snapshotting (copying stages into an approval instance at submit time) means reordering/editing a
/// workflow never disturbs in-flight approvals (req 12).
/// </summary>
public sealed class Workflow : IAggregateRoot
{
    private readonly List<WorkflowStage> _stages = new();

    public int Id { get; private set; }
    public int DocumentTypeId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public int Version { get; private set; } = 1;
    public bool IsActive { get; private set; }
    public int CreatedByEmployeeId { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    /// <summary>Backing-collection navigation (EF uses the field). Order by <see cref="WorkflowStage.StageOrder"/> at call sites.</summary>
    public IReadOnlyCollection<WorkflowStage> Stages => _stages.AsReadOnly();

    /// <summary>Stages in execution order.</summary>
    public IReadOnlyList<WorkflowStage> OrderedStages => _stages.OrderBy(s => s.StageOrder).ToList();

    private Workflow() { }

    public Workflow(int documentTypeId, string name, int createdByEmployeeId, DateTime createdAtUtc, int version = 1)
    {
        DocumentTypeId = documentTypeId;
        Name = name;
        CreatedByEmployeeId = createdByEmployeeId;
        CreatedAtUtc = createdAtUtc;
        Version = version;
    }

    public WorkflowStage AddStage(string name, ApproverAssignment approver, int? slaHours = null)
    {
        var order = _stages.Count == 0 ? 1 : _stages.Max(s => s.StageOrder) + 1;
        var stage = new WorkflowStage(order, name, approver, slaHours);
        _stages.Add(stage);
        return stage;
    }

    public void UpdateStage(int stageId, string name, ApproverAssignment approver, int? slaHours)
        => RequireStage(stageId).Update(name, approver, slaHours);

    public void RemoveStage(int stageId)
    {
        var stage = RequireStage(stageId);
        _stages.Remove(stage);
        Normalize();
    }

    /// <summary>Reorder stages to match the given full ordering of stage ids (req 12).</summary>
    public void Reorder(IReadOnlyList<int> orderedStageIds)
    {
        if (orderedStageIds.Count != _stages.Count ||
            orderedStageIds.Distinct().Count() != _stages.Count ||
            orderedStageIds.Any(id => _stages.All(s => s.Id != id)))
            throw new DomainException("Reorder must include every existing stage id exactly once.");

        for (var i = 0; i < orderedStageIds.Count; i++)
            RequireStage(orderedStageIds[i]).SetOrder(i + 1);
    }

    public void Rename(string name) => Name = name;

    public void Activate()
    {
        if (_stages.Count == 0)
            throw new DomainException("A workflow must have at least one stage before it can be activated.");
        IsActive = true;
    }

    public void Deactivate() => IsActive = false;

    /// <summary>Re-number stage orders to a contiguous 1..N sequence, preserving relative order.</summary>
    private void Normalize()
    {
        var ordered = _stages.OrderBy(s => s.StageOrder).ToList();
        for (var i = 0; i < ordered.Count; i++)
            ordered[i].SetOrder(i + 1);
    }

    private WorkflowStage RequireStage(int stageId)
        => _stages.FirstOrDefault(s => s.Id == stageId)
           ?? throw new DomainException($"Stage {stageId} does not belong to this workflow.");
}
