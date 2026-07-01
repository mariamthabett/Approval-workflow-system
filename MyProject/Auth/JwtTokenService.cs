using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using MyProject.Core.Application.Abstractions;
using MyProject.Core.Domain.Organization;

namespace MyProject.Api.Auth;

/// <summary>Issues HS256-signed JWTs carrying employee id, department, and role claims.</summary>
public sealed class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _settings;

    public JwtTokenService(JwtSettings settings) => _settings = settings;

    public string CreateToken(Employee employee, IReadOnlyCollection<string> roleCodes)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SigningKey));
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _settings.Issuer,
            Audience = _settings.Audience,
            Expires = DateTime.UtcNow.AddMinutes(_settings.ExpiryMinutes),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256),
            Claims = new Dictionary<string, object>
            {
                [AppClaims.EmployeeId] = employee.Id,
                [AppClaims.DepartmentId] = employee.DepartmentId,
                [AppClaims.Role] = roleCodes.ToArray()
            }
        };
        return new JsonWebTokenHandler().CreateToken(descriptor);
    }
}
