using MyProject.Core.Application.Documents;

namespace MyProject.Api.Endpoints;

/// <summary>Sample document module. Owns the Invoice; delegates approval to the generic engine.</summary>
public static class InvoiceEndpoints
{
    public static void MapInvoiceEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/invoices").WithTags("Invoices").RequireAuthorization();

        group.MapPost("/", async (CreateInvoiceRequest req, InvoiceService svc, CancellationToken ct) =>
        {
            var created = await svc.CreateAsync(req, ct);
            return Results.Created($"/api/invoices/{created.Id}", created);
        });

        group.MapGet("/{id:int}", async (int id, InvoiceService svc, CancellationToken ct) =>
            Results.Ok(await svc.GetAsync(id, ct)));

        group.MapPut("/{id:int}", async (int id, UpdateInvoiceRequest req, InvoiceService svc, CancellationToken ct) =>
            Results.Ok(await svc.UpdateAsync(id, req, ct)));

        group.MapPost("/{id:int}/submit", async (int id, InvoiceService svc, CancellationToken ct) =>
            Results.Ok(await svc.SubmitAsync(id, ct)));
    }
}
