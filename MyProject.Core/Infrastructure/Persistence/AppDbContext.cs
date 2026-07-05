using Microsoft.EntityFrameworkCore;
using MyProject.Core.Application.Abstractions;
using MyProject.Core.Domain.Approvals;
using MyProject.Core.Domain.Auditing;
using MyProject.Core.Domain.Documents;
using MyProject.Core.Domain.Notifications;
using MyProject.Core.Domain.Organization;
using MyProject.Core.Domain.Outbox;
using MyProject.Core.Domain.Workflows;

namespace MyProject.Core.Infrastructure.Persistence;

/// <summary>EF Core unit of work. Implements <see cref="IAppDbContext"/> so the application layer depends on the abstraction.</summary>
public sealed class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<EmployeeRole> EmployeeRoles => Set<EmployeeRole>();
    public DbSet<DocumentType> DocumentTypes => Set<DocumentType>();
    public DbSet<Workflow> Workflows => Set<Workflow>();
    public DbSet<WorkflowStage> WorkflowStages => Set<WorkflowStage>();
    public DbSet<ApprovalInstance> ApprovalInstances => Set<ApprovalInstance>();
    public DbSet<StageInstance> StageInstances => Set<StageInstance>();
    public DbSet<ApprovalAction> ApprovalActions => Set<ApprovalAction>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Provider-specific model configuration. SQL Server is the production target; SQLite is the
        // zero-install dev/verification provider.
        var isSqlServer = Database.IsSqlServer();

        // Optimistic concurrency: SQL Server has a native rowversion; SQLite does not, so we skip the
        // token there (single-writer dev use). Everything else behaves identically.
        if (isSqlServer)
            modelBuilder.Entity<ApprovalInstance>().Property(i => i.RowVersion).IsRowVersion();
        else
            modelBuilder.Entity<ApprovalInstance>().Ignore(i => i.RowVersion);

        // "At most one active workflow per document type" — a partial (filtered) unique index. Both
        // providers support it; only the identifier-quoting in the filter SQL differs.
        var activeFilter = isSqlServer ? "[IsActive] = 1" : "\"IsActive\" = 1";
        modelBuilder.Entity<Workflow>()
            .HasIndex(w => w.DocumentTypeId)
            .IsUnique()
            .HasFilter(activeFilter)
            .HasDatabaseName("UX_Workflow_ActivePerDocumentType");

        base.OnModelCreating(modelBuilder);
    }

    public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken = default)
    {
        // Retry-on-failure is disabled, so a manually-managed transaction is safe here.
        await using var tx = await Database.BeginTransactionAsync(cancellationToken);
        await operation(cancellationToken);
        await tx.CommitAsync(cancellationToken);
    }
}
