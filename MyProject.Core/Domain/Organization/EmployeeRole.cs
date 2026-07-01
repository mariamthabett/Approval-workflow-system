namespace MyProject.Core.Domain.Organization;

/// <summary>Join entity for the Employee &lt;-&gt; Role many-to-many relationship.</summary>
public sealed class EmployeeRole
{
    public int EmployeeId { get; private set; }
    public int RoleId { get; private set; }

    public Role? Role { get; private set; }

    private EmployeeRole() { }

    public EmployeeRole(int employeeId, int roleId)
    {
        EmployeeId = employeeId;
        RoleId = roleId;
    }
}
