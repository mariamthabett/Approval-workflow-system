using Microsoft.EntityFrameworkCore;
using MyProject.Core.Application.Abstractions;
using MyProject.Core.Domain.Documents;
using MyProject.Core.Domain.Organization;
using MyProject.Core.Domain.Workflows;

namespace MyProject.Core.Infrastructure.Persistence;

/// <summary>
/// Seeds a demoable dataset: five roles, two departments, five employees, and active 3-stage
/// LeaveRequest and Invoice workflows that exercise all three approver types (Department → Role → User).
/// Each block is independently idempotent, so it is safe to run on every startup.
/// </summary>
public static class DataSeeder
{
    /// <summary>Default password given to every seeded demo account so the demo login works out of the box.</summary>
    public const string DefaultPassword = "Password123!";

    public static async Task SeedAsync(AppDbContext db, IPasswordHasher passwordHasher, CancellationToken ct = default)
    {
        await SeedCoreAsync(db, passwordHasher, ct);
        await SeedInvoiceWorkflowAsync(db, ct);
    }

    /// <summary>Roles, departments, employees, and the LeaveRequest workflow. Skipped once employees exist.</summary>
    private static async Task SeedCoreAsync(AppDbContext db, IPasswordHasher passwordHasher, CancellationToken ct)
    {
        if (await db.Employees.AnyAsync(ct)) return;

        var now = DateTime.UtcNow;
        var defaultHash = passwordHasher.Hash(DefaultPassword);

        var rEmployee = new Role("Employee", "Employee");
        var rManager = new Role("Manager", "Manager");
        var rDeptHead = new Role("DeptHead", "Department Head");
        var rHr = new Role("HR", "HR Officer");
        var rAdmin = new Role("WorkflowAdmin", "Workflow Administrator");
        db.Roles.AddRange(rEmployee, rManager, rDeptHead, rHr, rAdmin);

        var engineering = new Department("Engineering");
        var humanResources = new Department("Human Resources");
        db.Departments.AddRange(engineering, humanResources);
        await db.SaveChangesAsync(ct);

        var alice = new Employee("Alice Employee", "alice@example.com", engineering.Id);   // initiator
        var bob = new Employee("Bob Manager", "bob@example.com", engineering.Id);           // Engineering manager
        var carol = new Employee("Carol DeptHead", "carol@example.com", engineering.Id);    // dept head
        var dan = new Employee("Dan HR", "dan@example.com", humanResources.Id);             // HR officer
        var admin = new Employee("Admin User", "admin@example.com", humanResources.Id);     // workflow admin
        foreach (var e in new[] { alice, bob, carol, dan, admin })
            e.SetPasswordHash(defaultHash);
        db.Employees.AddRange(alice, bob, carol, dan, admin);
        await db.SaveChangesAsync(ct);

        engineering.SetManager(bob.Id);
        alice.AssignRole(rEmployee.Id);
        bob.AssignRole(rManager.Id);
        carol.AssignRole(rDeptHead.Id);
        dan.AssignRole(rHr.Id);
        admin.AssignRole(rAdmin.Id);
        await db.SaveChangesAsync(ct);

        var docType = new DocumentType(LeaveRequest.DocumentTypeCode, "Leave Request");
        db.DocumentTypes.Add(docType);
        await db.SaveChangesAsync(ct);

        var workflow = new Workflow(docType.Id, "Standard Leave Approval", admin.Id, now);
        workflow.AddStage("Manager Approval", ApproverAssignment.ForDepartment(engineering.Id), slaHours: 48);
        workflow.AddStage("Department Head", ApproverAssignment.ForRole(rDeptHead.Id), slaHours: 48);
        workflow.AddStage("HR Approval", ApproverAssignment.ForUser(dan.Id), slaHours: 72);
        db.Workflows.Add(workflow);
        await db.SaveChangesAsync(ct);

        workflow.Activate();
        await db.SaveChangesAsync(ct);
    }

    /// <summary>
    /// Registers the "Invoice" document type and an active Manager → Department Head → HR approval workflow.
    /// Independently idempotent (keyed on the document type) and reuses the core-seeded org, so it runs even
    /// on an already-populated database.
    /// </summary>
    private static async Task SeedInvoiceWorkflowAsync(AppDbContext db, CancellationToken ct)
    {
        if (await db.DocumentTypes.AnyAsync(d => d.Code == Invoice.DocumentTypeCode, ct)) return;

        var engineering = await db.Departments.FirstOrDefaultAsync(d => d.Name == "Engineering", ct);
        var deptHead = await db.Roles.FirstOrDefaultAsync(r => r.Code == "DeptHead", ct);
        var dan = await db.Employees.FirstOrDefaultAsync(e => e.Email == "dan@example.com", ct);
        var admin = await db.Employees.FirstOrDefaultAsync(e => e.Email == "admin@example.com", ct);
        if (engineering is null || deptHead is null || dan is null || admin is null) return;

        var docType = new DocumentType(Invoice.DocumentTypeCode, "Invoice");
        db.DocumentTypes.Add(docType);
        await db.SaveChangesAsync(ct);

        var workflow = new Workflow(docType.Id, "Standard Invoice Approval", admin.Id, DateTime.UtcNow);
        workflow.AddStage("Manager Approval", ApproverAssignment.ForDepartment(engineering.Id), slaHours: 48);
        workflow.AddStage("Department Head", ApproverAssignment.ForRole(deptHead.Id), slaHours: 48);
        workflow.AddStage("HR Approval", ApproverAssignment.ForUser(dan.Id), slaHours: 72);
        db.Workflows.Add(workflow);
        await db.SaveChangesAsync(ct);

        workflow.Activate();
        await db.SaveChangesAsync(ct);
    }
}
