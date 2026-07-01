using Microsoft.EntityFrameworkCore;
using MyProject.Core.Application.Abstractions;
using MyProject.Core.Application.Approvals;
using MyProject.Core.Domain.Documents;

namespace MyProject.Core.Infrastructure.Outbox;

/// <summary>
/// The LeaveRequest module's subscription to engine events. It locks the document on submit/resubmit and
/// unlocks + sets the final status on approve/reject/cancel. This is how a document type integrates with
/// the generic engine — the engine has no compile-time knowledge of LeaveRequest. Adding a new document
/// type means adding another handler like this, with zero engine changes (Open/Closed).
/// </summary>
public sealed class LeaveRequestLockHandler : IIntegrationEventHandler
{
    private readonly IAppDbContext _db;

    public LeaveRequestLockHandler(IAppDbContext db) => _db = db;

    public bool CanHandle(string eventType, ApprovalEventPayload payload)
        => payload.DocumentTypeCode == LeaveRequest.DocumentTypeCode;

    public async Task HandleAsync(ApprovalEventPayload payload, CancellationToken ct = default)
    {
        if (!int.TryParse(payload.DocumentId, out var leaveId)) return;
        var leave = await _db.LeaveRequests.FirstOrDefaultAsync(l => l.Id == leaveId, ct);
        if (leave is null) return;

        switch (payload.EventType)
        {
            case ApprovalEventTypes.Submitted:
            case ApprovalEventTypes.Resubmitted:
                leave.OnSubmitted();
                break;
            case ApprovalEventTypes.Approved:
                leave.OnApproved();
                break;
            case ApprovalEventTypes.Rejected:
                leave.OnRejected();
                break;
            case ApprovalEventTypes.Cancelled:
                leave.OnCancelled();
                break;
        }
    }
}
