using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyProject.Core.Domain.Approvals;

namespace MyProject.Core.Infrastructure.Persistence.Configurations;

public sealed class ApprovalInstanceConfig : IEntityTypeConfiguration<ApprovalInstance>
{
    public void Configure(EntityTypeBuilder<ApprovalInstance> b)
    {
        b.ToTable("ApprovalInstances");
        b.HasKey(i => i.Id);
        b.Property(i => i.DocumentId).HasMaxLength(100).IsRequired();
        // RowVersion concurrency token is configured per-provider in AppDbContext.OnModelCreating
        // (SQL Server rowversion vs. SQLite which has no native equivalent).

        b.HasMany(i => i.Stages).WithOne().HasForeignKey(s => s.ApprovalInstanceId)
            .OnDelete(DeleteBehavior.Cascade);
        b.HasMany(i => i.Actions).WithOne().HasForeignKey(a => a.ApprovalInstanceId)
            .OnDelete(DeleteBehavior.Cascade);
        b.Metadata.FindNavigation(nameof(ApprovalInstance.Stages))!.SetPropertyAccessMode(PropertyAccessMode.Field);
        b.Metadata.FindNavigation(nameof(ApprovalInstance.Actions))!.SetPropertyAccessMode(PropertyAccessMode.Field);

        b.Ignore(i => i.CurrentStage);
        b.Ignore(i => i.OrderedStages);
        b.Ignore(i => i.OrderedActions);

        b.HasIndex(i => new { i.DocumentTypeId, i.DocumentId }).IsUnique();
        b.HasIndex(i => new { i.InitiatorEmployeeId, i.Status });
    }
}

public sealed class StageInstanceConfig : IEntityTypeConfiguration<StageInstance>
{
    public void Configure(EntityTypeBuilder<StageInstance> b)
    {
        b.ToTable("StageInstances", t => t.HasCheckConstraint("CK_StageInstance_Approver",
            "([ApproverType] = 1 AND [ApproverRoleId] IS NOT NULL) OR " +
            "([ApproverType] = 2 AND [ApproverDepartmentId] IS NOT NULL) OR " +
            "([ApproverType] = 3 AND [ResolvedApproverEmployeeId] IS NOT NULL)"));

        b.HasKey(s => s.Id);
        b.Property(s => s.Name).HasMaxLength(200).IsRequired();
        b.Ignore(s => s.DueAtUtc);
        b.HasIndex(s => new { s.ApprovalInstanceId, s.Status });
    }
}

public sealed class ApprovalActionConfig : IEntityTypeConfiguration<ApprovalAction>
{
    public void Configure(EntityTypeBuilder<ApprovalAction> b)
    {
        b.ToTable("ApprovalActions");
        b.HasKey(a => a.Id);
        b.Property(a => a.Comment).HasMaxLength(2000);
        b.HasIndex(a => new { a.ApprovalInstanceId, a.CreatedAtUtc });
    }
}
