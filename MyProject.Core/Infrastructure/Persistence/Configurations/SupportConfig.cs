using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
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
