using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MyProject.Core.Domain.Approvals;

namespace MyProject.Core.Infrastructure.Persistence;

/// <summary>
/// Enforces the immutable approval history (req 9) at the application boundary: any attempt to modify or
/// delete an <see cref="ApprovalAction"/> is rejected before it reaches the database. Defense-in-depth
/// alongside the database permission that denies UPDATE/DELETE on the audit table.
/// </summary>
public sealed class AuditImmutabilityInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Guard(eventData);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        Guard(eventData);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void Guard(DbContextEventData eventData)
    {
        var context = eventData.Context;
        if (context is null) return;

        var violation = context.ChangeTracker.Entries<ApprovalAction>()
            .Any(e => e.State is EntityState.Modified or EntityState.Deleted);

        if (violation)
            throw new InvalidOperationException(
                "Approval history is immutable: ApprovalAction records cannot be modified or deleted.");
    }
}
