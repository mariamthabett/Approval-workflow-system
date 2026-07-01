namespace MyProject.Core.Application.Abstractions;

/// <summary>A notification channel. Multiple implementations (in-app, email) can be registered.</summary>
public interface INotificationSender
{
    Task SendAsync(int recipientEmployeeId, long? approvalInstanceId, string title, string body, CancellationToken cancellationToken = default);
}
