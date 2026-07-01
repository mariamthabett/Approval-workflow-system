using MyProject.Core.Domain.Common;
using MyProject.Core.Domain.Enums;

namespace MyProject.Core.Domain.Workflows;

/// <summary>
/// Value object: to whom a stage is assigned. Exactly one reference is populated, matching
/// <see cref="ApproverType"/>. Immutable — replace rather than mutate.
/// </summary>
public sealed record ApproverAssignment
{
    public ApproverType Type { get; }
    public int? RoleId { get; }
    public int? DepartmentId { get; }
    public int? EmployeeId { get; }

    private ApproverAssignment(ApproverType type, int? roleId, int? departmentId, int? employeeId)
    {
        Type = type;
        RoleId = roleId;
        DepartmentId = departmentId;
        EmployeeId = employeeId;
    }

    public static ApproverAssignment ForRole(int roleId) => new(ApproverType.Role, roleId, null, null);
    public static ApproverAssignment ForDepartment(int departmentId) => new(ApproverType.Department, null, departmentId, null);
    public static ApproverAssignment ForUser(int employeeId) => new(ApproverType.User, null, null, employeeId);

    /// <summary>Rebuild from persisted columns, validating the "exactly one reference" invariant.</summary>
    public static ApproverAssignment FromColumns(ApproverType type, int? roleId, int? departmentId, int? employeeId)
        => type switch
        {
            ApproverType.Role when roleId is > 0 => ForRole(roleId.Value),
            ApproverType.Department when departmentId is > 0 => ForDepartment(departmentId.Value),
            ApproverType.User when employeeId is > 0 => ForUser(employeeId.Value),
            _ => throw new DomainException($"Invalid approver assignment: type {type} requires exactly one matching reference.")
        };
}
