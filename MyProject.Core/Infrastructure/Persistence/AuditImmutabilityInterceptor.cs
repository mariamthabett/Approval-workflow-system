using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MyProject.Core.Domain.Approvals;
using MyProject.Core.Domain.Auditing;

namespace MyProject.Core.Infrastructure.Persistence;

/// <summary>
/// Enforces the append-only audit trails at the application boundary: any attempt to modify or delete an
/// <see cref="ApprovalAction"/> (approval history) or an <see cref="ActivityLog"/> (per-user activity feed)
/// is rejected before it reaches the database. Defense-in-depth alongside database permissions that deny
/// UPDATE/DELETE on those tables. Inserts are always allowed.
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

        GuardEntity<ApprovalAction>(context, "Approval history", "ApprovalAction");
        GuardEntity<ActivityLog>(context, "The activity log", "ActivityLog");
    }

    private static void GuardEntity<TEntity>(DbContext context, string label, string typeName) where TEntity : class
    {
        var violation = context.ChangeTracker.Entries<TEntity>()
            .Any(e => e.State is EntityState.Modified or EntityState.Deleted);

        if (violation)
            throw new InvalidOperationException(
                $"{label} is immutable: {typeName} records cannot be modified or deleted.");
    }
}
