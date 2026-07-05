using Microsoft.EntityFrameworkCore;
using MyProject.Core.Application.Abstractions;
using MyProject.Core.Application.Approvals;
using MyProject.Core.Domain.Documents;

namespace MyProject.Core.Infrastructure.Outbox;

/// <summary>
/// The Invoice module's subscription to engine events. Locks the document on submit/resubmit and unlocks +
/// sets the final status on approve/reject/cancel. Mirrors <see cref="LeaveRequestLockHandler"/>: this is
/// the only wiring a new document type needs to integrate with the generic engine (Open/Closed).
/// </summary>
public sealed class InvoiceLockHandler : IIntegrationEventHandler
{
    private readonly IAppDbContext _db;

    public InvoiceLockHandler(IAppDbContext db) => _db = db;

    public bool CanHandle(string eventType, ApprovalEventPayload payload)
        => payload.DocumentTypeCode == Invoice.DocumentTypeCode;

    public async Task HandleAsync(ApprovalEventPayload payload, CancellationToken ct = default)
    {
        if (!int.TryParse(payload.DocumentId, out var invoiceId)) return;
        var invoice = await _db.Invoices.FirstOrDefaultAsync(i => i.Id == invoiceId, ct);
        if (invoice is null) return;

        switch (payload.EventType)
        {
            case ApprovalEventTypes.Submitted:
            case ApprovalEventTypes.Resubmitted:
                invoice.OnSubmitted();
                break;
            case ApprovalEventTypes.Approved:
                invoice.OnApproved();
                break;
            case ApprovalEventTypes.Rejected:
                invoice.OnRejected();
                break;
            case ApprovalEventTypes.Cancelled:
                invoice.OnCancelled();
                break;
        }
    }
}
