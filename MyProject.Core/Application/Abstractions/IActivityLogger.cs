using MyProject.Core.Domain.Enums;

namespace MyProject.Core.Application.Abstractions;

/// <summary>Appends a single entry to the immutable per-user activity log and persists it immediately.</summary>
public interface IActivityLogger
{
    Task LogAsync(
        ActivityType type,
        int? employeeId,
        string? actorEmail,
        string? description,
        string? entityType = null,
        string? entityId = null,
        string? ipAddress = null,
        CancellationToken cancellationToken = default);
}
