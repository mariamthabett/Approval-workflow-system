using Microsoft.EntityFrameworkCore;
using MyProject.Core.Application.Abstractions;
using MyProject.Core.Application.Approvals;

namespace MyProject.Core.Infrastructure.Outbox;

/// <summary>
/// Turns approval integration events into notifications: routes "approval required" to the current
/// stage's approvers, and terminal outcomes (approved/rejected/cancelled) to the initiator.
/// </summary>
public sealed class NotificationIntegrationHandler : IIntegrationEventHandler
{
    private readonly IAppDbContext _db;
    private readonly IApproverResolver _resolver;
    private readonly IEnumerable<INotificationSender> _senders;

    public NotificationIntegrationHandler(IAppDbContext db, IApproverResolver resolver, IEnumerable<INotificationSender> senders)
    {
        _db = db;
        _resolver = resolver;
        _senders = senders;
    }

    public bool CanHandle(string eventType, ApprovalEventPayload payload) => eventType switch
    {
        ApprovalEventTypes.Submitted or ApprovalEventTypes.Resubmitted or ApprovalEventTypes.StageAdvanced
            or ApprovalEventTypes.Approved or ApprovalEventTypes.Rejected or ApprovalEventTypes.Cancelled => true,
        _ => false
    };

    public async Task HandleAsync(ApprovalEventPayload payload, CancellationToken ct = default)
    {
        switch (payload.EventType)
        {
            case ApprovalEventTypes.Submitted:
            case ApprovalEventTypes.Resubmitted:
            case ApprovalEventTypes.StageAdvanced:
                await NotifyCurrentApproversAsync(payload, ct);
                break;

            case ApprovalEventTypes.Approved:
                await NotifyAsync(payload.InitiatorEmployeeId, payload.InstanceId,
                    "Document approved", $"{payload.DocumentTypeCode}/{payload.DocumentId} has been fully approved.", ct);
                break;

            case ApprovalEventTypes.Rejected:
                await NotifyAsync(payload.InitiatorEmployeeId, payload.InstanceId,
                    "Document rejected", $"{payload.DocumentTypeCode}/{payload.DocumentId} was rejected. Reason: {payload.Reason}", ct);
                break;

            case ApprovalEventTypes.Cancelled:
                await NotifyAsync(payload.InitiatorEmployeeId, payload.InstanceId,
                    "Approval cancelled", $"The approval for {payload.DocumentTypeCode}/{payload.DocumentId} was cancelled.", ct);
                break;
        }
    }

    private async Task NotifyCurrentApproversAsync(ApprovalEventPayload payload, CancellationToken ct)
    {
        if (payload.CurrentStageOrder is not int order) return;

        var stage = await _db.StageInstances.FirstOrDefaultAsync(
            s => s.ApprovalInstanceId == payload.InstanceId && s.CycleNumber == payload.CycleNumber && s.StageOrder == order, ct);
        if (stage is null) return;

        var approverIds = await _resolver.ResolveApproverEmployeeIdsAsync(stage, ct);
        foreach (var approverId in approverIds)
            await NotifyAsync(approverId, payload.InstanceId,
                "Approval required",
                $"{payload.DocumentTypeCode}/{payload.DocumentId} is awaiting your approval at stage '{payload.CurrentStageName}'.", ct);
    }

    private async Task NotifyAsync(int recipientEmployeeId, long instanceId, string title, string body, CancellationToken ct)
    {
        foreach (var sender in _senders)
            await sender.SendAsync(recipientEmployeeId, instanceId, title, body, ct);
    }
}
