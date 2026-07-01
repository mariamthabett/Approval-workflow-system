namespace MyProject.Core.Application.Abstractions;

/// <summary>The authenticated caller, projected from JWT claims.</summary>
public interface ICurrentUser
{
    bool IsAuthenticated { get; }

    /// <summary>The caller's employee id. Throws if unauthenticated.</summary>
    int EmployeeId { get; }

    int? DepartmentId { get; }

    IReadOnlyCollection<string> Roles { get; }

    bool IsInRole(string roleCode);
}
