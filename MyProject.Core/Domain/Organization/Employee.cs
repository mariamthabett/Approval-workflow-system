namespace MyProject.Core.Domain.Organization;

/// <summary>A person who can initiate documents and/or act as an approver.</summary>
public sealed class Employee
{
    private readonly List<EmployeeRole> _roles = new();

    public int Id { get; private set; }
    public string FullName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public int DepartmentId { get; private set; }
    public bool IsActive { get; private set; } = true;

    /// <summary>PBKDF2 password hash (see <c>IPasswordHasher</c>). Null for accounts without a set password.</summary>
    public string? PasswordHash { get; private set; }

    public Department? Department { get; private set; }
    public IReadOnlyCollection<EmployeeRole> Roles => _roles.AsReadOnly();

    private Employee() { }

    public Employee(string fullName, string email, int departmentId)
    {
        FullName = fullName;
        Email = email;
        DepartmentId = departmentId;
    }

    public void AssignRole(int roleId)
    {
        if (_roles.Any(r => r.RoleId == roleId)) return;
        _roles.Add(new EmployeeRole(Id, roleId));
    }

    /// <summary>Stores an already-hashed password. Hashing is the caller's responsibility (see <c>IPasswordHasher</c>).</summary>
    public void SetPasswordHash(string passwordHash) => PasswordHash = passwordHash;
}
