namespace MyProject.Core.Domain.Organization;

/// <summary>An organizational unit. Approver-type "Department" resolves to the manager (or members).</summary>
public sealed class Department
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public int? ManagerEmployeeId { get; private set; }
    public int? ParentDepartmentId { get; private set; }

    private Department() { }

    public Department(string name, int? parentDepartmentId = null)
    {
        Name = name;
        ParentDepartmentId = parentDepartmentId;
    }

    public void SetManager(int? managerEmployeeId) => ManagerEmployeeId = managerEmployeeId;
}
