using MyProject.Core.Application.Abstractions;
using MyProject.Core.Domain.Notifications;

namespace MyProject.Core.Infrastructure.Notifications;

/// <summary>
/// In-app notification channel: persists a <see cref="Notification"/> row on the shared scoped context.
/// The outbox dispatcher commits it. Additional channels (email/SMS) can be registered alongside.
/// </summary>
public sealed class InAppNotificationSender : INotificationSender
{
    private readonly IAppDbContext _db;
    private readonly IClock _clock;

    public InAppNotificationSender(IAppDbContext db, IClock clock)
    {
        _db = db;
        _clock = clock;
    }

    public Task SendAsync(int recipientEmployeeId, long? approvalInstanceId, string title, string body, CancellationToken ct = default)
    {
        _db.Notifications.Add(new Notification(recipientEmployeeId, approvalInstanceId, title, body, _clock.UtcNow));
        return Task.CompletedTask;
    }
}
