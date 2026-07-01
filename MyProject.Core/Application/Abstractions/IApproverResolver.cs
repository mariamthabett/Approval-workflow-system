using MyProject.Core.Domain.Approvals;

namespace MyProject.Core.Application.Abstractions;

/// <summary>
/// Resolves and authorizes approvers for a stage. This is the single gate that enforces
/// "only the current approver can act" (req 11). Implementation lives in Infrastructure because it
/// needs role/department membership from the database; the strategy is pluggable (delegation,
/// org-hierarchy escalation, etc.).
/// </summary>
public interface IApproverResolver
{
    /// <summary>True if <paramref name="employeeId"/> is entitled to act on this stage.</summary>
    Task<bool> CanActAsync(StageInstance stage, int employeeId, CancellationToken cancellationToken = default);

    /// <summary>The set of employees currently entitled to act on this stage (for notifications/queues).</summary>
    Task<IReadOnlyList<int>> ResolveApproverEmployeeIdsAsync(StageInstance stage, CancellationToken cancellationToken = default);
}
