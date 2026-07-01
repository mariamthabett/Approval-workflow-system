namespace MyProject.Core.Domain.Enums;

/// <summary>Who a workflow stage is assigned to.</summary>
public enum ApproverType
{
    Role = 1,
    Department = 2,
    User = 3
}

/// <summary>Lifecycle status of a running approval instance (mirrors the business document).</summary>
public enum InstanceStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3,
    Cancelled = 4
}

/// <summary>Status of a single stage within an approval cycle.</summary>
public enum StageStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3
}

/// <summary>The kind of action recorded in the immutable approval history.</summary>
public enum ActionType
{
    Submit = 1,
    Approve = 2,
    Reject = 3,
    Comment = 4,
    Resubmit = 5,
    Cancel = 6
}

/// <summary>Display/lock status of the sample LeaveRequest document.</summary>
public enum LeaveStatus
{
    Draft = 1,
    Submitted = 2,
    Approved = 3,
    Rejected = 4,
    Cancelled = 5
}
