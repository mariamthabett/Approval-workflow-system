using Microsoft.EntityFrameworkCore;
using MyProject.Core.Application.Abstractions;
using MyProject.Core.Domain.Enums;

namespace MyProject.Core.Application.Auditing;

public sealed record ActivityLogDto(
    long Id,
    int? EmployeeId,
    string? ActorName,
    string? ActorEmail,
    string Type,
    string? EntityType,
    string? EntityId,
    string? Description,
    string? IpAddress,
    DateTime CreatedAtUtc);

/// <summary>Read-side queries over the immutable per-user activity log.</summary>
public sealed class ActivityQueryService
{
    private const int MaxTake = 200;

    private readonly IAppDbContext _db;

    public ActivityQueryService(IAppDbContext db) => _db = db;

    /// <summary>Most recent activity for a single employee (their own feed).</summary>
    public Task<IReadOnlyList<ActivityLogDto>> ForEmployeeAsync(int employeeId, int take, CancellationToken ct)
        => QueryAsync(employeeId, type: null, skip: 0, take, ct);

    /// <summary>Admin feed: all activity, optionally filtered by employee and/or type, newest first.</summary>
    public Task<IReadOnlyList<ActivityLogDto>> ListAsync(
        int? employeeId, ActivityType? type, int skip, int take, CancellationToken ct)
        => QueryAsync(employeeId, type, skip, take, ct);

    private async Task<IReadOnlyList<ActivityLogDto>> QueryAsync(
        int? employeeId, ActivityType? type, int skip, int take, CancellationToken ct)
    {
        take = Math.Clamp(take, 1, MaxTake);
        skip = Math.Max(skip, 0);

        var query = _db.ActivityLogs.AsQueryable();
        if (employeeId is { } eid) query = query.Where(a => a.EmployeeId == eid);
        if (type is { } t) query = query.Where(a => a.Type == t);

        // Left-join Employees so entries keep an actor name even when only the id was recorded.
        var rows = await query
            .OrderByDescending(a => a.CreatedAtUtc)
            .Skip(skip)
            .Take(take)
            .Select(a => new
            {
                a.Id, a.EmployeeId, a.ActorEmail, a.Type, a.EntityType,
                a.EntityId, a.Description, a.IpAddress, a.CreatedAtUtc,
                ActorName = _db.Employees.Where(e => e.Id == a.EmployeeId).Select(e => e.FullName).FirstOrDefault()
            })
            .ToListAsync(ct);

        return rows.Select(a => new ActivityLogDto(
            a.Id, a.EmployeeId, a.ActorName, a.ActorEmail, a.Type.ToString(),
            a.EntityType, a.EntityId, a.Description, a.IpAddress, a.CreatedAtUtc)).ToList();
    }
}
