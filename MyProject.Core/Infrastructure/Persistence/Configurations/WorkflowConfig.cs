using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyProject.Core.Domain.Workflows;

namespace MyProject.Core.Infrastructure.Persistence.Configurations;

public sealed class DocumentTypeConfig : IEntityTypeConfiguration<DocumentType>
{
    public void Configure(EntityTypeBuilder<DocumentType> b)
    {
        b.ToTable("DocumentTypes");
        b.HasKey(d => d.Id);
        b.Property(d => d.Code).HasMaxLength(64).IsRequired();
        b.Property(d => d.Name).HasMaxLength(200).IsRequired();
        b.HasIndex(d => d.Code).IsUnique();
    }
}

public sealed class WorkflowConfig : IEntityTypeConfiguration<Workflow>
{
    public void Configure(EntityTypeBuilder<Workflow> b)
    {
        b.ToTable("Workflows");
        b.HasKey(w => w.Id);
        b.Property(w => w.Name).HasMaxLength(200).IsRequired();

        b.HasMany(w => w.Stages).WithOne().HasForeignKey(s => s.WorkflowId)
            .OnDelete(DeleteBehavior.Cascade);
        b.Metadata.FindNavigation(nameof(Workflow.Stages))!.SetPropertyAccessMode(PropertyAccessMode.Field);
        b.Ignore(w => w.OrderedStages);

        b.HasOne<DocumentType>().WithMany().HasForeignKey(w => w.DocumentTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // The "at most one active workflow per document type" partial unique index (req 13) is added
        // per-provider in AppDbContext.OnModelCreating (the filter SQL differs between SQL Server and SQLite).
        b.HasIndex(w => new { w.DocumentTypeId, w.Version }).IsUnique();
    }
}

public sealed class WorkflowStageConfig : IEntityTypeConfiguration<WorkflowStage>
{
    public void Configure(EntityTypeBuilder<WorkflowStage> b)
    {
        b.ToTable("WorkflowStages", t => t.HasCheckConstraint("CK_WorkflowStage_Approver",
            "([ApproverType] = 1 AND [ApproverRoleId] IS NOT NULL AND [ApproverDepartmentId] IS NULL AND [ApproverEmployeeId] IS NULL) OR " +
            "([ApproverType] = 2 AND [ApproverDepartmentId] IS NOT NULL AND [ApproverRoleId] IS NULL AND [ApproverEmployeeId] IS NULL) OR " +
            "([ApproverType] = 3 AND [ApproverEmployeeId] IS NOT NULL AND [ApproverRoleId] IS NULL AND [ApproverDepartmentId] IS NULL)"));

        b.HasKey(s => s.Id);
        b.Property(s => s.Name).HasMaxLength(200).IsRequired();
        b.Ignore(s => s.Approver);
        // Non-unique: reordering swaps StageOrder values, which would transiently collide under a unique
        // index (SQL Server has no deferred constraints). Uniqueness is guaranteed by the Workflow aggregate.
        b.HasIndex(s => new { s.WorkflowId, s.StageOrder });
    }
}
