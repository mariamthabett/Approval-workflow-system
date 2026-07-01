using MyProject.Core.Domain.Organization;

namespace MyProject.Core.Application.Abstractions;

/// <summary>Issues signed JWTs carrying the employee id, department and role claims.</summary>
public interface IJwtTokenService
{
    string CreateToken(Employee employee, IReadOnlyCollection<string> roleCodes);
}
