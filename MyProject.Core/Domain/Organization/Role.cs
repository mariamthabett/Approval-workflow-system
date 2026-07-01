namespace MyProject.Core.Domain.Organization;

/// <summary>A security/business role that can be assigned to employees and to workflow stages.</summary>
public sealed class Role
{
    public int Id { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;

    private Role() { }

    public Role(string code, string name)
    {
        Code = code;
        Name = name;
    }
}
