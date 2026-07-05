using MyProject.Core.Domain.Documents;

namespace MyProject.Core.Application.Documents;

public sealed record CreateInvoiceRequest(
    decimal Amount, string Currency, string Vendor, string? Description, DateOnly InvoiceDate, DateOnly DueDate);

public sealed record UpdateInvoiceRequest(
    decimal Amount, string Currency, string Vendor, string? Description, DateOnly InvoiceDate, DateOnly DueDate);

public sealed record InvoiceDto(
    int Id, int OwnerEmployeeId, decimal Amount, string Currency, string Vendor, string? Description,
    DateOnly InvoiceDate, DateOnly DueDate, string Status, bool IsLocked, DateTime CreatedAtUtc);

public static class InvoiceMappings
{
    public static InvoiceDto ToDto(this Invoice i) => new(
        i.Id, i.OwnerEmployeeId, i.Amount, i.Currency, i.Vendor, i.Description,
        i.InvoiceDate, i.DueDate, i.Status.ToString(), i.IsLocked, i.CreatedAtUtc);
}
