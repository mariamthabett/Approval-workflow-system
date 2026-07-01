using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MyProject.Core.Application.Abstractions;
using MyProject.Core.Application.Common;
using MyProject.Core.Domain.Approvals;
using MyProject.Core.Domain.Outbox;
using MyProject.Core.Domain.Workflows;

namespace MyProject.Core.Application.Approvals;

/// <summary>
/// Orchestrates the approval use-cases. Pattern for every write: load → authorize → invoke the domain
/// aggregate → enqueue integration event(s) to the outbox → persist atomically. All business rules and
/// state transitions live in <see cref="ApprovalInstance"/>; this service only coordinates.
/// </summary>
public sealed class ApprovalAppService
{
    private readonly IAppDbContext _db;
    private readonly IClock _clock;
    private readonly ICurrentUser _currentUser;
    private readonly IApproverResolver _resolver;

    public ApprovalAppService(IAppDbContext db, IClock clock, ICurrentUser currentUser, IApproverResolver resolver)
    {
        _db = db;
        _clock = clock;
        _currentUser = currentUser;
        _resolver = resolver;
    }

    public async Task<ApprovalInstanceDto> SubmitAsync(SubmitApprovalRequest req, CancellationToken ct)
    {
        var initiator = _currentUser.EmployeeId;
        var docType = await RequireDocumentTypeAsync(req.DocumentTypeCode, ct);

        var exists = await _db.ApprovalInstances
            .AnyAsync(i => i.DocumentTypeId == docType.Id && i.DocumentId == req.DocumentId, ct);
        if (exists)
            throw new ConflictException($"An approval already exists for {docType.Code}/{req.DocumentId}. Use resubmit to restart.");

        var workflow = await LoadActiveWorkflowAsync(docType, ct);
        var defs = workflow.OrderedStages.Select(StageDefinition.From).ToList();

        var instance = ApprovalInstance.Start(docType.Id, req.DocumentId, workflow.Id, workflow.Version, defs, initiator, _clock.UtcNow);
        _db.ApprovalInstances.Add(instance);

        // Two-phase inside one transaction: the outbox payload needs the generated instance id.
        await _db.ExecuteInTransactionAsync(async token =>
        {
            await _db.SaveChangesAsync(token);
            Enqueue(instance, docType, ApprovalEventTypes.Submitted, reason: null);
            await _db.SaveChangesAsync(token);
        }, ct);

        return instance.ToDto();
    }

    public async Task<ApprovalInstanceDto> ApproveAsync(long instanceId, ApproveRequest req, CancellationToken ct)
    {
        var instance = await RequireInstanceAsync(instanceId, ct);
        var docType = await RequireDocumentTypeByIdAsync(instance.DocumentTypeId, ct);
        await AuthorizeCurrentApproverAsync(instance, ct);

        var outcome = instance.Approve(_currentUser.EmployeeId, req.Comment, _clock.UtcNow);
        Enqueue(instance, docType,
            outcome == ApprovalOutcome.Completed ? ApprovalEventTypes.Approved : ApprovalEventTypes.StageAdvanced,
            reason: null);

        await _db.SaveChangesAsync(ct);
        return instance.ToDto();
    }

    public async Task<ApprovalInstanceDto> RejectAsync(long instanceId, RejectRequest req, CancellationToken ct)
    {
        var instance = await RequireInstanceAsync(instanceId, ct);
        var docType = await RequireDocumentTypeByIdAsync(instance.DocumentTypeId, ct);
        await AuthorizeCurrentApproverAsync(instance, ct);

        instance.Reject(_currentUser.EmployeeId, req.Comment, _clock.UtcNow);
        Enqueue(instance, docType, ApprovalEventTypes.Rejected, reason: req.Comment);

        await _db.SaveChangesAsync(ct);
        return instance.ToDto();
    }

    public async Task<ApprovalInstanceDto> CommentAsync(long instanceId, CommentRequest req, CancellationToken ct)
    {
        var instance = await RequireInstanceAsync(instanceId, ct);
        var isApprover = instance.CurrentStage is not null &&
                         await _resolver.CanActAsync(instance.CurrentStage, _currentUser.EmployeeId, ct);
        if (!isApprover && instance.InitiatorEmployeeId != _currentUser.EmployeeId)
            throw new ForbiddenException("Only the current approver or the initiator may comment.");

        instance.AddComment(_currentUser.EmployeeId, req.Comment, _clock.UtcNow);
        await _db.SaveChangesAsync(ct);
        return instance.ToDto();
    }

    public async Task<ApprovalInstanceDto> ResubmitAsync(long instanceId, CancellationToken ct)
    {
        var instance = await RequireInstanceAsync(instanceId, ct);
        if (instance.InitiatorEmployeeId != _currentUser.EmployeeId)
            throw new ForbiddenException("Only the initiator can resubmit the document.");

        var docType = await RequireDocumentTypeByIdAsync(instance.DocumentTypeId, ct);
        var workflow = await LoadActiveWorkflowAsync(docType, ct);
        var defs = workflow.OrderedStages.Select(StageDefinition.From).ToList();

        instance.Resubmit(defs, _currentUser.EmployeeId, _clock.UtcNow);
        Enqueue(instance, docType, ApprovalEventTypes.Resubmitted, reason: null);

        await _db.SaveChangesAsync(ct);
        return instance.ToDto();
    }

    public async Task<ApprovalInstanceDto> CancelAsync(long instanceId, CancelRequest req, CancellationToken ct)
    {
        var instance = await RequireInstanceAsync(instanceId, ct);
        if (instance.InitiatorEmployeeId != _currentUser.EmployeeId && !_currentUser.IsInRole("WorkflowAdmin"))
            throw new ForbiddenException("Only the initiator or an administrator can cancel the approval.");

        var docType = await RequireDocumentTypeByIdAsync(instance.DocumentTypeId, ct);
        instance.Cancel(_currentUser.EmployeeId, req.Comment, _clock.UtcNow);
        Enqueue(instance, docType, ApprovalEventTypes.Cancelled, reason: req.Comment);

        await _db.SaveChangesAsync(ct);
        return instance.ToDto();
    }

    public async Task<ApprovalInstanceDto> GetAsync(long instanceId, CancellationToken ct)
        => (await RequireInstanceAsync(instanceId, ct)).ToDto();

    public async Task<IReadOnlyList<ApprovalActionDto>> GetHistoryAsync(long instanceId, CancellationToken ct)
    {
        var instance = await _db.ApprovalInstances
            .Include(i => i.Actions)
            .FirstOrDefaultAsync(i => i.Id == instanceId, ct)
            ?? throw new NotFoundException($"Approval instance {instanceId} was not found.");
        return instance.OrderedActions.Select(a => a.ToDto()).ToList();
    }

    // ---- helpers ----

    private async Task AuthorizeCurrentApproverAsync(ApprovalInstance instance, CancellationToken ct)
    {
        var stage = instance.CurrentStage
            ?? throw new ConflictException($"The approval is not awaiting a decision (status: {instance.Status}).");
        if (!await _resolver.CanActAsync(stage, _currentUser.EmployeeId, ct))
            throw new ForbiddenException("You are not the current approver for this stage.");
    }

    private void Enqueue(ApprovalInstance instance, DocumentType docType, string eventType, string? reason)
    {
        var payload = new ApprovalEventPayload(
            instance.Id, eventType, docType.Id, docType.Code, instance.DocumentId,
            instance.InitiatorEmployeeId, instance.CycleNumber,
            instance.CurrentStageOrder, instance.CurrentStage?.Name, reason);
        _db.OutboxMessages.Add(new OutboxMessage(instance.Id, eventType, JsonSerializer.Serialize(payload), _clock.UtcNow));
    }

    private async Task<ApprovalInstance> RequireInstanceAsync(long instanceId, CancellationToken ct)
        => await _db.ApprovalInstances
               .Include(i => i.Stages)
               .FirstOrDefaultAsync(i => i.Id == instanceId, ct)
           ?? throw new NotFoundException($"Approval instance {instanceId} was not found.");

    private async Task<DocumentType> RequireDocumentTypeAsync(string code, CancellationToken ct)
        => await _db.DocumentTypes.FirstOrDefaultAsync(d => d.Code == code && d.IsActive, ct)
           ?? throw new NotFoundException($"Active document type '{code}' was not found.");

    private async Task<DocumentType> RequireDocumentTypeByIdAsync(int id, CancellationToken ct)
        => await _db.DocumentTypes.FirstOrDefaultAsync(d => d.Id == id, ct)
           ?? throw new NotFoundException($"Document type {id} was not found.");

    private async Task<Workflow> LoadActiveWorkflowAsync(DocumentType docType, CancellationToken ct)
        => await _db.Workflows.Include(w => w.Stages)
               .FirstOrDefaultAsync(w => w.DocumentTypeId == docType.Id && w.IsActive, ct)
           ?? throw new ConflictException($"No active workflow is configured for document type '{docType.Code}'.");
}
