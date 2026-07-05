using MyProject.Core.Domain.Enums;

namespace MyProject.Core.Domain.Auditing;

/// <summary>
/// A single immutable entry in the per-user activity log. Append-only: once written it is never updated
/// or deleted (enforced by <c>AuditImmutabilityInterceptor</c>). Records who did what, when, from where,
/// and against which entity. This is a high-level cross-cutting feed (logins, registrations, decisions),
/// distinct from the detailed per-approval <c>ApprovalAction</c> history.
/// </summary>
public sealed class ActivityLog
{
    public long Id { get; private set; }

    /// <summary>The acting employee, when known. Null for events like a failed login with an unknown email.</summary>
    public int? EmployeeId { get; private set; }

    /// <summary>Denormalized actor email captured at write time (may be an unregistered address for failed logins).</summary>
    public string? ActorEmail { get; private set; }

    public ActivityType Type { get; private set; }

    /// <summary>Optional target entity, e.g. "LeaveRequest" or "ApprovalInstance".</summary>
    public string? EntityType { get; private set; }
    public string? EntityId { get; private set; }

    public string? Description { get; private set; }
    public string? IpAddress { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private ActivityLog() { }

    public ActivityLog(
        ActivityType type,
        int? employeeId,
        string? actorEmail,
        string? description,
        string? entityType,
        string? entityId,
        string? ipAddress,
        DateTime createdAtUtc)
    {
        Type = type;
        EmployeeId = employeeId;
        ActorEmail = actorEmail;
        Description = description;
        EntityType = entityType;
        EntityId = entityId;
        IpAddress = ipAddress;
        CreatedAtUtc = createdAtUtc;
    }
}
