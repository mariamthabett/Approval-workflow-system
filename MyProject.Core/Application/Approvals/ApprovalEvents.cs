namespace MyProject.Core.Application.Approvals;

/// <summary>Integration-event type names written to the outbox and matched by subscribers.</summary>
public static class ApprovalEventTypes
{
    public const string Submitted = "DocumentSubmitted";
    public const string Resubmitted = "DocumentResubmitted";
    public const string StageAdvanced = "StageAdvanced";
    public const string Approved = "InstanceApproved";
    public const string Rejected = "InstanceRejected";
    public const string Cancelled = "InstanceCancelled";
}

/// <summary>
/// Payload serialized into an <c>OutboxMessage</c>. Deliberately carries only what subscribers need to
/// act without re-coupling to the engine: which document, which stage is now current, and (for
/// rejections) the reason. The authoritative instance id is the outbox row's ApprovalInstanceId.
/// </summary>
public sealed record ApprovalEventPayload(
    long InstanceId,
    string EventType,
    int DocumentTypeId,
    string DocumentTypeCode,
    string DocumentId,
    int InitiatorEmployeeId,
    int CycleNumber,
    int? CurrentStageOrder,
    string? CurrentStageName,
    string? Reason);
