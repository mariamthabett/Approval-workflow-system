using MyProject.Core.Domain.Common;
using MyProject.Core.Domain.Enums;

namespace MyProject.Core.Domain.Documents;

/// <summary>
/// Sample business document: a vendor invoice raised by an employee and routed through the generic
/// approval engine. Like <see cref="LeaveRequest"/>, it owns its own data and lock state and links to the
/// engine only by the loose (DocumentType, DocumentId) reference, reacting to engine events to lock/unlock
/// itself. The engine never references this type — adding it required zero engine changes.
/// </summary>
public sealed class Invoice : IAggregateRoot
{
    /// <summary>Stable document-type code the engine uses to pick a workflow.</summary>
    public const string DocumentTypeCode = "Invoice";

    public int Id { get; private set; }
    public int OwnerEmployeeId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "EGP";
    public string Vendor { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DateOnly InvoiceDate { get; private set; }
    public DateOnly DueDate { get; private set; }
    public InvoiceStatus Status { get; private set; } = InvoiceStatus.Draft;
    public bool IsLocked { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private Invoice() { }

    public Invoice(
        int ownerEmployeeId, decimal amount, string currency, string vendor, string? description,
        DateOnly invoiceDate, DateOnly dueDate, DateTime createdAtUtc)
    {
        Validate(amount, currency, vendor, invoiceDate, dueDate);
        OwnerEmployeeId = ownerEmployeeId;
        Amount = amount;
        Currency = currency.Trim().ToUpperInvariant();
        Vendor = vendor.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        InvoiceDate = invoiceDate;
        DueDate = dueDate;
        CreatedAtUtc = createdAtUtc;
    }

    /// <summary>Owner edits are only allowed while Draft or Rejected (document is unlocked).</summary>
    public void Edit(decimal amount, string currency, string vendor, string? description, DateOnly invoiceDate, DateOnly dueDate)
    {
        if (IsLocked || (Status != InvoiceStatus.Draft && Status != InvoiceStatus.Rejected))
            throw new DomainException("Invoice can only be edited while in Draft or Rejected state.");
        Validate(amount, currency, vendor, invoiceDate, dueDate);
        Amount = amount;
        Currency = currency.Trim().ToUpperInvariant();
        Vendor = vendor.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        InvoiceDate = invoiceDate;
        DueDate = dueDate;
    }

    private static void Validate(decimal amount, string currency, string vendor, DateOnly invoiceDate, DateOnly dueDate)
    {
        if (amount <= 0) throw new DomainException("Amount must be greater than zero.");
        if (string.IsNullOrWhiteSpace(currency)) throw new DomainException("Currency is required.");
        if (string.IsNullOrWhiteSpace(vendor)) throw new DomainException("Vendor is required.");
        if (dueDate < invoiceDate) throw new DomainException("DueDate cannot be earlier than InvoiceDate.");
    }

    // --- reactions to engine integration events (invoked by the outbox subscriber) ---
    public void OnSubmitted() { IsLocked = true; Status = InvoiceStatus.Submitted; }
    public void OnApproved() { IsLocked = false; Status = InvoiceStatus.Approved; }
    public void OnRejected() { IsLocked = false; Status = InvoiceStatus.Rejected; }
    public void OnCancelled() { IsLocked = false; Status = InvoiceStatus.Cancelled; }
}
