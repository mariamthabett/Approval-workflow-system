using Microsoft.EntityFrameworkCore;
using MyProject.Core.Application.Abstractions;
using MyProject.Core.Domain.Enums;

namespace MyProject.Core.Application.Dashboards;

/// <summary>
/// Read-side queries for dashboards. "My pending" reuses <see cref="IApproverResolver"/> so the queue
/// matches authorization exactly; at scale this would be served by a denormalized approver-queue
/// projection (see plan §14/§15) rather than per-instance resolution.
/// </summary>
public sealed class DashboardQueryService
{
    private readonly IAppDbContext _db;
    private readonly IClock _clock;
    private readonly ICurrentUser _currentUser;
    private readonly IApproverResolver _resolver;

    public DashboardQueryService(IAppDbContext db, IClock clock, ICurrentUser currentUser, IApproverResolver resolver)
    {
        _db = db;
        _clock = clock;
        _currentUser = currentUser;
        _resolver = resolver;
    }

    public async Task<IReadOnlyList<PendingApprovalDto>> MyPendingAsync(CancellationToken ct)
    {
        var me = _currentUser.EmployeeId;
        var codes = await DocumentTypeCodesAsync(ct);
        var pending = await _db.ApprovalInstances
            .Include(i => i.Stages)
            .Where(i => i.Status == InstanceStatus.Pending)
            .ToListAsync(ct);

        var result = new List<PendingApprovalDto>();
        foreach (var instance in pending)
        {
            var stage = instance.CurrentStage;
            if (stage is null) continue;
            if (!await _resolver.CanActAsync(stage, me, ct)) continue;

            result.Add(new PendingApprovalDto(
                instance.Id, instance.DocumentTypeId, codes.GetValueOrDefault(instance.DocumentTypeId, "?"),
                instance.DocumentId, instance.CycleNumber, stage.StageOrder, stage.Name,
                instance.InitiatorEmployeeId, stage.EnteredAtUtc, stage.DueAtUtc,
                stage.DueAtUtc is { } due && due < _clock.UtcNow));
        }
        return result.OrderBy(r => r.EnteredAtUtc).ToList();
    }

    public async Task<IReadOnlyList<MyDocumentDto>> MyDocumentsAsync(CancellationToken ct)
    {
        var me = _currentUser.EmployeeId;
        var codes = await DocumentTypeCodesAsync(ct);
        var mine = await _db.ApprovalInstances
            .Include(i => i.Stages)
            .Where(i => i.InitiatorEmployeeId == me)
            .OrderByDescending(i => i.CreatedAtUtc)
            .ToListAsync(ct);

        return mine.Select(i => new MyDocumentDto(
            i.Id, i.DocumentTypeId, codes.GetValueOrDefault(i.DocumentTypeId, "?"), i.DocumentId,
            i.Status.ToString(), i.CycleNumber, i.CurrentStageOrder, i.CurrentStage?.Name,
            i.CreatedAtUtc, i.CompletedAtUtc)).ToList();
    }

    public async Task<IReadOnlyList<PendingApprovalDto>> SlaBreachesAsync(CancellationToken ct)
    {
        var codes = await DocumentTypeCodesAsync(ct);
        var pending = await _db.ApprovalInstances
            .Include(i => i.Stages)
            .Where(i => i.Status == InstanceStatus.Pending)
            .ToListAsync(ct);

        var now = _clock.UtcNow;
        return pending
            .Select(i => (i, stage: i.CurrentStage))
            .Where(x => x.stage is { } s && s.DueAtUtc is { } due && due < now)
            .Select(x => new PendingApprovalDto(
                x.i.Id, x.i.DocumentTypeId, codes.GetValueOrDefault(x.i.DocumentTypeId, "?"),
                x.i.DocumentId, x.i.CycleNumber, x.stage!.StageOrder, x.stage.Name,
                x.i.InitiatorEmployeeId, x.stage.EnteredAtUtc, x.stage.DueAtUtc, true))
            .OrderBy(r => r.DueAtUtc)
            .ToList();
    }

    public async Task<WorkflowMetricsDto> WorkflowMetricsAsync(int workflowId, CancellationToken ct)
    {
        var instances = await _db.ApprovalInstances
            .Where(i => i.WorkflowId == workflowId)
            .Select(i => new { i.Status, i.CreatedAtUtc, i.CompletedAtUtc })
            .ToListAsync(ct);

        var completed = instances
            .Where(i => i.Status == InstanceStatus.Approved && i.CompletedAtUtc != null)
            .Select(i => (i.CompletedAtUtc!.Value - i.CreatedAtUtc).TotalHours)
            .ToList();

        return new WorkflowMetricsDto(
            workflowId,
            instances.Count,
            instances.Count(i => i.Status == InstanceStatus.Pending),
            instances.Count(i => i.Status == InstanceStatus.Approved),
            instances.Count(i => i.Status == InstanceStatus.Rejected),
            instances.Count(i => i.Status == InstanceStatus.Cancelled),
            completed.Count > 0 ? completed.Average() : null);
    }

    private async Task<Dictionary<int, string>> DocumentTypeCodesAsync(CancellationToken ct)
        => await _db.DocumentTypes.ToDictionaryAsync(d => d.Id, d => d.Code, ct);
}
