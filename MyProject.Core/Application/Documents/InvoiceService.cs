using Microsoft.EntityFrameworkCore;
using MyProject.Core.Application.Abstractions;
using MyProject.Core.Application.Approvals;
using MyProject.Core.Application.Common;
using MyProject.Core.Domain.Documents;

namespace MyProject.Core.Application.Documents;

/// <summary>
/// Sample business module for vendor invoices. It owns the Invoice document and enforces owner-only
/// editing; approval is delegated entirely to the generic engine via <see cref="ApprovalAppService"/>.
/// Locking/unlocking happens reactively through engine events (see <c>InvoiceLockHandler</c>). Mirrors
/// <see cref="LeaveRequestService"/> — proving a new document type needs no engine changes.
/// </summary>
public sealed class InvoiceService
{
    private readonly IAppDbContext _db;
    private readonly IClock _clock;
    private readonly ICurrentUser _currentUser;
    private readonly ApprovalAppService _approvals;

    public InvoiceService(IAppDbContext db, IClock clock, ICurrentUser currentUser, ApprovalAppService approvals)
    {
        _db = db;
        _clock = clock;
        _currentUser = currentUser;
        _approvals = approvals;
    }

    public async Task<InvoiceDto> CreateAsync(CreateInvoiceRequest req, CancellationToken ct)
    {
        var invoice = new Invoice(
            _currentUser.EmployeeId, req.Amount, req.Currency, req.Vendor, req.Description,
            req.InvoiceDate, req.DueDate, _clock.UtcNow);
        _db.Invoices.Add(invoice);
        await _db.SaveChangesAsync(ct);
        return invoice.ToDto();
    }

    public async Task<InvoiceDto> UpdateAsync(int id, UpdateInvoiceRequest req, CancellationToken ct)
    {
        var invoice = await RequireOwnedAsync(id, ct);
        invoice.Edit(req.Amount, req.Currency, req.Vendor, req.Description, req.InvoiceDate, req.DueDate);
        await _db.SaveChangesAsync(ct);
        return invoice.ToDto();
    }

    public async Task<InvoiceDto> GetAsync(int id, CancellationToken ct)
    {
        var invoice = await _db.Invoices.FirstOrDefaultAsync(i => i.Id == id, ct)
            ?? throw new NotFoundException($"Invoice {id} was not found.");
        return invoice.ToDto();
    }

    /// <summary>Submit the invoice into the approval engine (starts the workflow).</summary>
    public async Task<ApprovalInstanceDto> SubmitAsync(int id, CancellationToken ct)
    {
        var invoice = await RequireOwnedAsync(id, ct);
        if (invoice.IsLocked)
            throw new ConflictException("Invoice is already in an approval process.");

        return await _approvals.SubmitAsync(
            new SubmitApprovalRequest(Invoice.DocumentTypeCode, invoice.Id.ToString()), ct);
    }

    private async Task<Invoice> RequireOwnedAsync(int id, CancellationToken ct)
    {
        var invoice = await _db.Invoices.FirstOrDefaultAsync(i => i.Id == id, ct)
            ?? throw new NotFoundException($"Invoice {id} was not found.");
        if (invoice.OwnerEmployeeId != _currentUser.EmployeeId)
            throw new ForbiddenException("You can only manage your own invoices.");
        return invoice;
    }
}
