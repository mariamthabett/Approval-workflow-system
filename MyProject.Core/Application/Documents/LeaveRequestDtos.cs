using MyProject.Core.Domain.Documents;

namespace MyProject.Core.Application.Documents;

public sealed record CreateLeaveRequest(DateOnly FromDate, DateOnly ToDate, string Reason);
public sealed record UpdateLeaveRequest(DateOnly FromDate, DateOnly ToDate, string Reason);

public sealed record LeaveRequestDto(
    int Id, int OwnerEmployeeId, DateOnly FromDate, DateOnly ToDate, string Reason,
    string Status, bool IsLocked, DateTime CreatedAtUtc);

public static class LeaveRequestMappings
{
    public static LeaveRequestDto ToDto(this LeaveRequest l) => new(
        l.Id, l.OwnerEmployeeId, l.FromDate, l.ToDate, l.Reason,
        l.Status.ToString(), l.IsLocked, l.CreatedAtUtc);
}
