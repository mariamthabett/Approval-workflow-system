using Microsoft.EntityFrameworkCore;
using MyProject.Core.Application.Abstractions;
using MyProject.Core.Domain.Approvals;
using MyProject.Core.Domain.Enums;

namespace MyProject.Core.Infrastructure.Approvals;

/// <summary>
/// Default approver-resolution strategy:
/// <list type="bullet">
/// <item><b>User</b>: the single named employee.</item>
/// <item><b>Role</b>: any active employee holding that role.</item>
/// <item><b>Department</b>: the department manager; if none is set, any active member.</item>
/// </list>
/// This is the single place that answers "who may act on this stage", used both for authorization
/// (req 11) and for building approver notifications/queues. Swap the registration to change the policy.
/// </summary>
public sealed class ApproverResolver : IApproverResolver
{
    private readonly IAppDbContext _db;

    public ApproverResolver(IAppDbContext db) => _db = db;

    public async Task<bool> CanActAsync(StageInstance stage, int employeeId, CancellationToken ct = default)
    {
        var employee = await _db.Employees.FirstOrDefaultAsync(e => e.Id == employeeId && e.IsActive, ct);
        if (employee is null) return false;

        return stage.ApproverType switch
        {
            ApproverType.User => stage.ResolvedApproverEmployeeId == employeeId,

            ApproverType.Role => stage.ApproverRoleId is int roleId
                && await _db.EmployeeRoles.AnyAsync(er => er.EmployeeId == employeeId && er.RoleId == roleId, ct),

            ApproverType.Department => stage.ApproverDepartmentId is int deptId
                && await IsDepartmentApproverAsync(deptId, employee.Id, ct),

            _ => false
        };
    }

    public async Task<IReadOnlyList<int>> ResolveApproverEmployeeIdsAsync(StageInstance stage, CancellationToken ct = default)
    {
        switch (stage.ApproverType)
        {
            case ApproverType.User:
                return stage.ResolvedApproverEmployeeId is int uid ? new[] { uid } : Array.Empty<int>();

            case ApproverType.Role:
                if (stage.ApproverRoleId is not int roleId) return Array.Empty<int>();
                return await _db.EmployeeRoles
                    .Where(er => er.RoleId == roleId)
                    .Join(_db.Employees.Where(e => e.IsActive), er => er.EmployeeId, e => e.Id, (er, e) => e.Id)
                    .Distinct().ToListAsync(ct);

            case ApproverType.Department:
                if (stage.ApproverDepartmentId is not int deptId) return Array.Empty<int>();
                var manager = await _db.Departments
                    .Where(d => d.Id == deptId)
                    .Select(d => d.ManagerEmployeeId)
                    .FirstOrDefaultAsync(ct);
                if (manager is int managerId)
                    return new[] { managerId };
                return await _db.Employees
                    .Where(e => e.DepartmentId == deptId && e.IsActive)
                    .Select(e => e.Id).ToListAsync(ct);

            default:
                return Array.Empty<int>();
        }
    }

    private async Task<bool> IsDepartmentApproverAsync(int departmentId, int employeeId, CancellationToken ct)
    {
        var manager = await _db.Departments
            .Where(d => d.Id == departmentId)
            .Select(d => d.ManagerEmployeeId)
            .FirstOrDefaultAsync(ct);

        if (manager is int managerId) return managerId == employeeId;

        // No manager configured — fall back to any active member of the department.
        return await _db.Employees.AnyAsync(e => e.Id == employeeId && e.DepartmentId == departmentId && e.IsActive, ct);
    }
}
