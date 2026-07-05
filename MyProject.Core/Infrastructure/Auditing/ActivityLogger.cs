using MyProject.Core.Application.Abstractions;
using MyProject.Core.Domain.Auditing;
using MyProject.Core.Domain.Enums;

namespace MyProject.Core.Infrastructure.Auditing;

/// <summary>
/// Writes activity-log entries through the shared <see cref="IAppDbContext"/>. Persists immediately so a
/// log entry survives even when the surrounding use-case does no other write (e.g. a failed login). Adding
/// a row is an insert, which the immutability interceptor permits.
/// </summary>
public sealed class ActivityLogger : IActivityLogger
{
    private readonly IAppDbContext _db;
    private readonly IClock _clock;

    public ActivityLogger(IAppDbContext db, IClock clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task LogAsync(
        ActivityType type,
        int? employeeId,
        string? actorEmail,
        string? description,
        string? entityType = null,
        string? entityId = null,
        string? ipAddress = null,
        CancellationToken cancellationToken = default)
    {
        _db.ActivityLogs.Add(new ActivityLog(
            type, employeeId, actorEmail, description, entityType, entityId, ipAddress, _clock.UtcNow));
        await _db.SaveChangesAsync(cancellationToken);
    }
}
