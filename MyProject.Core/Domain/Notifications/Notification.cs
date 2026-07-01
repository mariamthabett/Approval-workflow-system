namespace MyProject.Core.Domain.Notifications;

/// <summary>An in-app notification delivered to an employee (approval requested / decided).</summary>
public sealed class Notification
{
    public long Id { get; private set; }
    public int RecipientEmployeeId { get; private set; }
    public long? ApprovalInstanceId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;
    public bool IsRead { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private Notification() { }

    public Notification(int recipientEmployeeId, long? approvalInstanceId, string title, string body, DateTime createdAtUtc)
    {
        RecipientEmployeeId = recipientEmployeeId;
        ApprovalInstanceId = approvalInstanceId;
        Title = title;
        Body = body;
        CreatedAtUtc = createdAtUtc;
    }

    public void MarkRead() => IsRead = true;
}
