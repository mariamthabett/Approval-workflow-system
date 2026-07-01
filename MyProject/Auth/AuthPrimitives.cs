namespace MyProject.Api.Auth;

/// <summary>Custom JWT claim types used by the API.</summary>
public static class AppClaims
{
    public const string EmployeeId = "employeeId";
    public const string DepartmentId = "departmentId";
    public const string Role = "role";
}

/// <summary>Authorization policy names.</summary>
public static class Policies
{
    public const string WorkflowAdmin = "WorkflowAdmin";
}

/// <summary>JWT signing/validation settings bound from the "Jwt" configuration section.</summary>
public sealed class JwtSettings
{
    public string Issuer { get; set; } = "ApprovalWorkflow";
    public string Audience { get; set; } = "ApprovalWorkflow";
    public string SigningKey { get; set; } = string.Empty;
    public int ExpiryMinutes { get; set; } = 480;
}
