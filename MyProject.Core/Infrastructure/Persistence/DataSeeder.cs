using Microsoft.EntityFrameworkCore;
using MyProject.Core.Domain.Documents;
using MyProject.Core.Domain.Organization;
using MyProject.Core.Domain.Workflows;

namespace MyProject.Core.Infrastructure.Persistence;

/// <summary>
/// Seeds a demoable dataset: five roles, two departments, five employees, and an active 3-stage
/// LeaveRequest workflow that exercises all three approver types (Department → Role → User).
/// Idempotent: does nothing if employees already exist.
/// </summary>
public static class DataSeeder
{
    public static async Task SeedAsync(AppDbContext db, CancellationToken ct = default)
    {
        if (await db.Employees.AnyAsync(ct)) return;

        var now = DateTime.UtcNow;

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
}
