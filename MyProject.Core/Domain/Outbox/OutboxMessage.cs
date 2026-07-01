namespace MyProject.Core.Domain.Outbox;

/// <summary>
/// Transactional outbox row. The application enqueues integration events here inside the same
/// transaction as the approval state change; a background dispatcher delivers them to subscribers
/// (notifications, document lock/unlock). Guarantees at-least-once delivery without 2PC.
/// </summary>
public sealed class OutboxMessage
{
    public long Id { get; private set; }
    public long? ApprovalInstanceId { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public string PayloadJson { get; private set; } = string.Empty;
    public DateTime OccurredAtUtc { get; private set; }
    public DateTime? ProcessedAtUtc { get; private set; }
    public int Attempts { get; private set; }
    public string? Error { get; private set; }

    private OutboxMessage() { }

    public OutboxMessage(long? approvalInstanceId, string eventType, string payloadJson, DateTime occurredAtUtc)
    {
        ApprovalInstanceId = approvalInstanceId;
        EventType = eventType;
        PayloadJson = payloadJson;
        OccurredAtUtc = occurredAtUtc;
    }

    public void MarkProcessed(DateTime whenUtc)
    {
        ProcessedAtUtc = whenUtc;
        Error = null;
    }

    public void MarkFailed(string error)
    {
        Attempts++;
        Error = error;
    }
}
