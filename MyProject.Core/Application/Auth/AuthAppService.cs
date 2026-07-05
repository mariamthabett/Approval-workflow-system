using Microsoft.EntityFrameworkCore;
using MyProject.Core.Application.Abstractions;
using MyProject.Core.Application.Common;
using MyProject.Core.Domain.Enums;
using MyProject.Core.Domain.Organization;

namespace MyProject.Core.Application.Auth;

/// <summary>
/// User account use-cases: self-service registration, password login, password change, and admin-provisioned
/// accounts. Password hashing is delegated to <see cref="IPasswordHasher"/>; every outcome (including failed
/// logins) is recorded via <see cref="IActivityLogger"/>. Token issuance stays in the API layer.
/// </summary>
public sealed class AuthAppService
{
    private const string DefaultRoleCode = "Employee";
    private const int MinPasswordLength = 8;

    private readonly IAppDbContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly IActivityLogger _activity;

    public AuthAppService(IAppDbContext db, IPasswordHasher hasher, IActivityLogger activity)
    {
        _db = db;
        _hasher = hasher;
        _activity = activity;
    }

    /// <summary>Self-service sign-up. Creates the account, assigns the default "Employee" role, logs the user in.</summary>
    public async Task<AuthResult> RegisterAsync(RegisterRequest req, string? ipAddress, CancellationToken ct)
    {
        var email = Normalize(req.Email);
        ValidateCredentials(req.FullName, email, req.Password);
        await RequireUniqueEmailAsync(email, ct);
        await RequireDepartmentAsync(req.DepartmentId, ct);

        var employee = await CreateEmployeeAsync(req.FullName, email, req.Password, req.DepartmentId, ct);

        var defaultRole = await _db.Roles.FirstOrDefaultAsync(r => r.Code == DefaultRoleCode, ct);
        if (defaultRole is not null)
        {
            employee.AssignRole(defaultRole.Id);
            await _db.SaveChangesAsync(ct);
        }

        var roles = await RolesOfAsync(employee.Id, ct);
        await _activity.LogAsync(ActivityType.Register, employee.Id, email, "Registered a new account",
            ipAddress: ipAddress, cancellationToken: ct);
        return new AuthResult(employee, roles);
    }

    /// <summary>Verifies email + password. Returns null on invalid credentials (already logged as a failed attempt).</summary>
    public async Task<AuthResult?> LoginAsync(LoginRequest req, string? ipAddress, CancellationToken ct)
    {
        var email = Normalize(req.Email);
        var employee = await _db.Employees.FirstOrDefaultAsync(e => e.Email == email && e.IsActive, ct);

        if (employee?.PasswordHash is null || !_hasher.Verify(employee.PasswordHash, req.Password ?? string.Empty))
        {
            await _activity.LogAsync(ActivityType.LoginFailed, employee?.Id, email, "Failed login attempt",
                ipAddress: ipAddress, cancellationToken: ct);
            return null;
        }

        var roles = await RolesOfAsync(employee.Id, ct);
        await _activity.LogAsync(ActivityType.Login, employee.Id, email, "Signed in",
            ipAddress: ipAddress, cancellationToken: ct);
        return new AuthResult(employee, roles);
    }

    public async Task ChangePasswordAsync(int employeeId, ChangePasswordRequest req, string? ipAddress, CancellationToken ct)
    {
        var employee = await _db.Employees.FirstOrDefaultAsync(e => e.Id == employeeId, ct)
            ?? throw new NotFoundException($"Employee {employeeId} was not found.");

        if (string.IsNullOrWhiteSpace(req.NewPassword) || req.NewPassword.Length < MinPasswordLength)
            throw new ValidationException($"New password must be at least {MinPasswordLength} characters.");
        if (employee.PasswordHash is null || !_hasher.Verify(employee.PasswordHash, req.CurrentPassword ?? string.Empty))
            throw new ValidationException("Current password is incorrect.");

        employee.SetPasswordHash(_hasher.Hash(req.NewPassword));
        await _db.SaveChangesAsync(ct);
        await _activity.LogAsync(ActivityType.PasswordChanged, employee.Id, employee.Email, "Changed password",
            ipAddress: ipAddress, cancellationToken: ct);
    }

    /// <summary>Admin-provisioned account with an initial password and explicit roles.</summary>
    public async Task<UserSummary> AdminCreateUserAsync(
        AdminCreateUserRequest req, int adminEmployeeId, string? ipAddress, CancellationToken ct)
    {
        var email = Normalize(req.Email);
        ValidateCredentials(req.FullName, email, req.Password);
        await RequireUniqueEmailAsync(email, ct);
        await RequireDepartmentAsync(req.DepartmentId, ct);

        var roleIds = (req.RoleIds ?? Array.Empty<int>()).Distinct().ToList();
        if (roleIds.Count > 0)
        {
            var known = await _db.Roles.Where(r => roleIds.Contains(r.Id)).Select(r => r.Id).ToListAsync(ct);
            var missing = roleIds.Except(known).ToList();
            if (missing.Count > 0)
                throw new ValidationException($"Unknown role id(s): {string.Join(", ", missing)}.");
        }

        var employee = await CreateEmployeeAsync(req.FullName, email, req.Password, req.DepartmentId, ct);

        foreach (var roleId in roleIds) employee.AssignRole(roleId);
        if (roleIds.Count > 0) await _db.SaveChangesAsync(ct);

        var roles = await RolesOfAsync(employee.Id, ct);
        await _activity.LogAsync(ActivityType.AdminCreatedUser, adminEmployeeId, null, $"Created user {email}",
            entityType: "Employee", entityId: employee.Id.ToString(), ipAddress: ipAddress, cancellationToken: ct);
        return new UserSummary(employee.Id, employee.FullName, employee.Email, employee.DepartmentId, roles);
    }

    // ---- helpers ----

    private async Task<Employee> CreateEmployeeAsync(string fullName, string email, string password, int departmentId, CancellationToken ct)
    {
        var employee = new Employee(fullName.Trim(), email, departmentId);
        employee.SetPasswordHash(_hasher.Hash(password));
        _db.Employees.Add(employee);
        await _db.SaveChangesAsync(ct);   // AssignRole needs the generated id (mirrors DataSeeder)
        return employee;
    }

    private async Task RequireUniqueEmailAsync(string email, CancellationToken ct)
    {
        if (await _db.Employees.AnyAsync(e => e.Email == email, ct))
            throw new ConflictException($"An account with email '{email}' already exists.");
    }

    private async Task RequireDepartmentAsync(int departmentId, CancellationToken ct)
    {
        if (!await _db.Departments.AnyAsync(d => d.Id == departmentId, ct))
            throw new ValidationException($"Department {departmentId} was not found.");
    }

    private async Task<IReadOnlyCollection<string>> RolesOfAsync(int employeeId, CancellationToken ct)
        => await _db.EmployeeRoles
            .Where(er => er.EmployeeId == employeeId)
            .Join(_db.Roles, er => er.RoleId, r => r.Id, (er, r) => r.Code)
            .ToListAsync(ct);

    private static string Normalize(string? email) => (email ?? string.Empty).Trim().ToLowerInvariant();

    private static void ValidateCredentials(string? fullName, string email, string? password)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ValidationException("Full name is required.");
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new ValidationException("A valid email is required.");
        if (string.IsNullOrWhiteSpace(password) || password.Length < MinPasswordLength)
            throw new ValidationException($"Password must be at least {MinPasswordLength} characters.");
    }
}
