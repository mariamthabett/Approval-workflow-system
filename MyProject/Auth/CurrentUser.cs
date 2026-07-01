using System.Security.Claims;
using MyProject.Core.Application.Abstractions;
using MyProject.Core.Application.Common;

namespace MyProject.Api.Auth;

/// <summary>Projects <see cref="ICurrentUser"/> from the authenticated JWT principal.</summary>
public sealed class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _accessor;

    public CurrentUser(IHttpContextAccessor accessor) => _accessor = accessor;

    private ClaimsPrincipal? Principal => _accessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public int EmployeeId =>
        int.TryParse(Principal?.FindFirst(AppClaims.EmployeeId)?.Value, out var id)
            ? id
            : throw new ForbiddenException("The request is not authenticated.");

    public int? DepartmentId =>
        int.TryParse(Principal?.FindFirst(AppClaims.DepartmentId)?.Value, out var deptId) ? deptId : null;

    public IReadOnlyCollection<string> Roles =>
        Principal?.FindAll(AppClaims.Role).Select(c => c.Value).ToArray() ?? Array.Empty<string>();

    public bool IsInRole(string roleCode) => Roles.Contains(roleCode);
}
