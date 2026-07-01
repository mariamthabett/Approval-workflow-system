using MyProject.Core.Domain.Common;
using MyProject.Core.Domain.Enums;

namespace MyProject.Core.Domain.Documents;

/// <summary>
/// Sample business document proving the engine is generic. It owns its own data and lock state;
/// it links to the approval engine only by the loose (DocumentType, DocumentId) reference and
/// reacts to engine events to lock/unlock itself. The engine never references this type.
/// </summary>
public sealed class LeaveRequest : IAggregateRoot
{
    /// <summary>Stable document-type code the engine uses to pick a workflow.</summary>
    public const string DocumentTypeCode = "LeaveRequest";

    public int Id { get; private set; }
    public int OwnerEmployeeId { get; private set; }
    public DateOnly FromDate { get; private set; }
    public DateOnly ToDate { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public LeaveStatus Status { get; private set; } = LeaveStatus.Draft;
    public bool IsLocked { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private LeaveRequest() { }

    public LeaveRequest(int ownerEmployeeId, DateOnly fromDate, DateOnly toDate, string reason, DateTime createdAtUtc)
    {
        if (toDate < fromDate) throw new DomainException("ToDate cannot be earlier than FromDate.");
        OwnerEmployeeId = ownerEmployeeId;
        FromDate = fromDate;
        ToDate = toDate;
        Reason = reason;
        CreatedAtUtc = createdAtUtc;
    }

    /// <summary>Owner edits are only allowed while Draft or Rejected (document is unlocked).</summary>
    public void Edit(DateOnly fromDate, DateOnly toDate, string reason)
    {
        if (IsLocked || (Status != LeaveStatus.Draft && Status != LeaveStatus.Rejected))
            throw new DomainException("Leave request can only be edited while in Draft or Rejected state.");
        if (toDate < fromDate) throw new DomainException("ToDate cannot be earlier than FromDate.");
        FromDate = fromDate;
        ToDate = toDate;
        Reason = reason;
    }

    // --- reactions to engine integration events (invoked by the outbox subscriber) ---
    public void OnSubmitted() { IsLocked = true; Status = LeaveStatus.Submitted; }
    public void OnApproved() { IsLocked = false; Status = LeaveStatus.Approved; }
    public void OnRejected() { IsLocked = false; Status = LeaveStatus.Rejected; }
    public void OnCancelled() { IsLocked = false; Status = LeaveStatus.Cancelled; }
}
