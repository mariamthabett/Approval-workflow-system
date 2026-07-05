using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyProject.Core.Domain.Auditing;
using MyProject.Core.Domain.Documents;
using MyProject.Core.Domain.Notifications;
using MyProject.Core.Domain.Outbox;

namespace MyProject.Core.Infrastructure.Persistence.Configurations;

public sealed class OutboxMessageConfig : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> b)
    {
        b.ToTable("OutboxMessages");
        b.HasKey(m => m.Id);
        b.Property(m => m.EventType).HasMaxLength(128).IsRequired();
        b.Property(m => m.PayloadJson).IsRequired();
        b.Property(m => m.Error).HasMaxLength(4000);
        b.HasIndex(m => m.ProcessedAtUtc);
    }
}

public sealed class NotificationConfig : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> b)
    {
        b.ToTable("Notifications");
        b.HasKey(n => n.Id);
        b.Property(n => n.Title).HasMaxLength(256).IsRequired();
        b.Property(n => n.Body).HasMaxLength(2000).IsRequired();
        b.HasIndex(n => new { n.RecipientEmployeeId, n.IsRead });
    }
}

public sealed class LeaveRequestConfig : IEntityTypeConfiguration<LeaveRequest>
{
    public void Configure(EntityTypeBuilder<LeaveRequest> b)
    {
        b.ToTable("LeaveRequests");
        b.HasKey(l => l.Id);
        b.Property(l => l.Reason).HasMaxLength(1000).IsRequired();
        b.HasIndex(l => l.OwnerEmployeeId);
    }
}

public sealed class ActivityLogConfig : IEntityTypeConfiguration<ActivityLog>
{
    public void Configure(EntityTypeBuilder<ActivityLog> b)
    {
        b.ToTable("ActivityLogs");
        b.HasKey(a => a.Id);
        b.Property(a => a.Type).HasConversion<int>().IsRequired();
        b.Property(a => a.ActorEmail).HasMaxLength(256);
        b.Property(a => a.EntityType).HasMaxLength(128);
        b.Property(a => a.EntityId).HasMaxLength(64);
        b.Property(a => a.Description).HasMaxLength(1000);
        b.Property(a => a.IpAddress).HasMaxLength(64);
        b.HasIndex(a => a.EmployeeId);
        b.HasIndex(a => a.CreatedAtUtc);
    }
}
