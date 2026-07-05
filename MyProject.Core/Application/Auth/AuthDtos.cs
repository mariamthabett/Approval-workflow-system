using MyProject.Core.Domain.Organization;

namespace MyProject.Core.Application.Auth;

public sealed record RegisterRequest(string FullName, string Email, string Password, int DepartmentId);

public sealed record LoginRequest(string Email, string Password);

public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);

public sealed record AdminCreateUserRequest(
    string FullName, string Email, string Password, int DepartmentId, IReadOnlyCollection<int>? RoleIds);

/// <summary>Result of a successful authentication — the employee plus role codes, used by the API to mint a JWT.</summary>
public sealed record AuthResult(Employee Employee, IReadOnlyCollection<string> Roles);

public sealed record UserSummary(
    int Id, string FullName, string Email, int DepartmentId, IReadOnlyCollection<string> Roles);
