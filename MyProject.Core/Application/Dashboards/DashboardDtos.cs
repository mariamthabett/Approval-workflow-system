namespace MyProject.Core.Application.Dashboards;

/// <summary>An item in an approver's "waiting on me" queue.</summary>
public sealed record PendingApprovalDto(
    long InstanceId, int DocumentTypeId, string DocumentTypeCode, string DocumentId,
    int CycleNumber, int StageOrder, string StageName,
    int InitiatorEmployeeId, DateTime EnteredAtUtc, DateTime? DueAtUtc, bool IsOverdue);

/// <summary>A document the caller initiated, with its latest approval state.</summary>
public sealed record MyDocumentDto(
    long InstanceId, int DocumentTypeId, string DocumentTypeCode, string DocumentId,
    string Status, int CycleNumber, int? CurrentStageOrder, string? CurrentStageName,
    DateTime CreatedAtUtc, DateTime? CompletedAtUtc);

/// <summary>Aggregate throughput metrics for one workflow.</summary>
public sealed record WorkflowMetricsDto(
    int WorkflowId, int Total, int Pending, int Approved, int Rejected, int Cancelled,
    double? AverageCycleTimeHours);
