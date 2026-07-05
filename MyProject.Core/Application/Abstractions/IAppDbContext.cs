using Microsoft.EntityFrameworkCore;
using MyProject.Core.Domain.Approvals;
using MyProject.Core.Domain.Auditing;
using MyProject.Core.Domain.Documents;
using MyProject.Core.Domain.Notifications;
using MyProject.Core.Domain.Organization;
using MyProject.Core.Domain.Outbox;
using MyProject.Core.Domain.Workflows;

namespace MyProject.Core.Application.Abstractions;

/// <summary>
/// Persistence gateway exposed to the application layer. Kept as an interface so services depend on an
/// abstraction (testable) rather than the concrete EF <c>DbContext</c>.
/// </summary>
public interface IAppDbContext
{
    DbSet<Department> Departments { get; }
    DbSet<Employee> Employees { get; }
    DbSet<Role> Roles { get; }
    DbSet<EmployeeRole> EmployeeRoles { get; }
    DbSet<DocumentType> DocumentTypes { get; }
    DbSet<Workflow> Workflows { get; }
    DbSet<WorkflowStage> WorkflowStages { get; }
    DbSet<ApprovalInstance> ApprovalInstances { get; }
    DbSet<StageInstance> StageInstances { get; }
    DbSet<ApprovalAction> ApprovalActions { get; }
    DbSet<OutboxMessage> OutboxMessages { get; }
    DbSet<Notification> Notifications { get; }
    DbSet<LeaveRequest> LeaveRequests { get; }
    DbSet<Invoice> Invoices { get; }
    DbSet<ActivityLog> ActivityLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>Run a write unit atomically (used where two SaveChanges must share one transaction).</summary>
    Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken = default);
}
