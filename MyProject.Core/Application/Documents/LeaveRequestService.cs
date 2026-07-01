using Microsoft.EntityFrameworkCore;
using MyProject.Core.Application.Abstractions;
using MyProject.Core.Application.Approvals;
using MyProject.Core.Application.Common;
using MyProject.Core.Domain.Documents;

namespace MyProject.Core.Application.Documents;

/// <summary>
/// Sample business module. It owns the LeaveRequest document and enforces owner-only editing; approval
/// is delegated entirely to the generic engine via <see cref="ApprovalAppService"/>. Locking/unlocking
/// happens reactively through engine events (see the outbox subscriber), proving the engine never needs
/// to know about this type.
/// </summary>
public sealed class LeaveRequestService
{
    private readonly IAppDbContext _db;
    private readonly IClock _clock;
    private readonly ICurrentUser _currentUser;
    private readonly ApprovalAppService _approvals;

    public LeaveRequestService(IAppDbContext db, IClock clock, ICurrentUser currentUser, ApprovalAppService approvals)
    {
        _db = db;
        _clock = clock;
        _currentUser = currentUser;
        _approvals = approvals;
    }

    public async Task<LeaveRequestDto> CreateAsync(CreateLeaveRequest req, CancellationToken ct)
    {
        var leave = new LeaveRequest(_currentUser.EmployeeId, req.FromDate, req.ToDate, req.Reason, _clock.UtcNow);
        _db.LeaveRequests.Add(leave);
        await _db.SaveChangesAsync(ct);
        return leave.ToDto();
    }

    public async Task<LeaveRequestDto> UpdateAsync(int id, UpdateLeaveRequest req, CancellationToken ct)
    {
        var leave = await RequireOwnedAsync(id, ct);
        leave.Edit(req.FromDate, req.ToDate, req.Reason);
        await _db.SaveChangesAsync(ct);
        return leave.ToDto();
    }

    public async Task<LeaveRequestDto> GetAsync(int id, CancellationToken ct)
    {
        var leave = await _db.LeaveRequests.FirstOrDefaultAsync(l => l.Id == id, ct)
            ?? throw new NotFoundException($"Leave request {id} was not found.");
        return leave.ToDto();
    }

    /// <summary>Submit the leave request into the approval engine (starts the workflow).</summary>
    public async Task<ApprovalInstanceDto> SubmitAsync(int id, CancellationToken ct)
    {
        var leave = await RequireOwnedAsync(id, ct);
        if (leave.IsLocked)
            throw new ConflictException("Leave request is already in an approval process.");

        return await _approvals.SubmitAsync(
            new SubmitApprovalRequest(LeaveRequest.DocumentTypeCode, leave.Id.ToString()), ct);
    }

    private async Task<LeaveRequest> RequireOwnedAsync(int id, CancellationToken ct)
    {
        var leave = await _db.LeaveRequests.FirstOrDefaultAsync(l => l.Id == id, ct)
            ?? throw new NotFoundException($"Leave request {id} was not found.");
        if (leave.OwnerEmployeeId != _currentUser.EmployeeId)
            throw new ForbiddenException("You can only manage your own leave requests.");
        return leave;
    }
}
