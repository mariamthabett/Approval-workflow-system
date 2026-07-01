using Microsoft.EntityFrameworkCore;
using MyProject.Core.Application.Abstractions;
using MyProject.Core.Application.Common;
using MyProject.Core.Domain.Enums;
using MyProject.Core.Domain.Workflows;

namespace MyProject.Core.Application.Workflows;

/// <summary>
/// Administrative use-cases for document types and workflow templates: create workflows, edit/reorder
/// stages (req 12), and activate a version (exactly one active workflow per document type).
/// </summary>
public sealed class WorkflowAdminService
{
    private readonly IAppDbContext _db;
    private readonly IClock _clock;
    private readonly ICurrentUser _currentUser;

    public WorkflowAdminService(IAppDbContext db, IClock clock, ICurrentUser currentUser)
    {
        _db = db;
        _clock = clock;
        _currentUser = currentUser;
    }

    // ---- document types ----

    public async Task<IReadOnlyList<DocumentTypeDto>> ListDocumentTypesAsync(CancellationToken ct)
        => await _db.DocumentTypes.OrderBy(d => d.Code).Select(d => d.ToDto()).ToListAsync(ct);

    public async Task<DocumentTypeDto> CreateDocumentTypeAsync(CreateDocumentTypeRequest req, CancellationToken ct)
    {
        if (await _db.DocumentTypes.AnyAsync(d => d.Code == req.Code, ct))
            throw new ConflictException($"Document type '{req.Code}' already exists.");
        var dt = new DocumentType(req.Code, req.Name);
        _db.DocumentTypes.Add(dt);
        await _db.SaveChangesAsync(ct);
        return dt.ToDto();
    }

    // ---- workflows ----

    public async Task<IReadOnlyList<WorkflowDto>> ListWorkflowsAsync(int? documentTypeId, CancellationToken ct)
    {
        var query = _db.Workflows.Include(w => w.Stages).AsQueryable();
        if (documentTypeId is int id) query = query.Where(w => w.DocumentTypeId == id);
        var workflows = await query.OrderBy(w => w.DocumentTypeId).ThenBy(w => w.Version).ToListAsync(ct);
        return workflows.Select(w => w.ToDto()).ToList();
    }

    public async Task<WorkflowDto> GetWorkflowAsync(int id, CancellationToken ct)
        => (await RequireWorkflowAsync(id, ct)).ToDto();

    public async Task<WorkflowDto> CreateWorkflowAsync(CreateWorkflowRequest req, CancellationToken ct)
    {
        if (!await _db.DocumentTypes.AnyAsync(d => d.Id == req.DocumentTypeId, ct))
            throw new NotFoundException($"Document type {req.DocumentTypeId} was not found.");

        var nextVersion = 1 + await _db.Workflows
            .Where(w => w.DocumentTypeId == req.DocumentTypeId)
            .Select(w => (int?)w.Version).MaxAsync(ct) ?? 1;

        var workflow = new Workflow(req.DocumentTypeId, req.Name, _currentUser.EmployeeId, _clock.UtcNow, nextVersion);
        _db.Workflows.Add(workflow);
        await _db.SaveChangesAsync(ct);
        return workflow.ToDto();
    }

    public async Task<WorkflowDto> RenameWorkflowAsync(int id, RenameWorkflowRequest req, CancellationToken ct)
    {
        var workflow = await RequireWorkflowAsync(id, ct);
        workflow.Rename(req.Name);
        await _db.SaveChangesAsync(ct);
        return workflow.ToDto();
    }

    public async Task<WorkflowDto> AddStageAsync(int workflowId, StageRequest req, CancellationToken ct)
    {
        var workflow = await RequireWorkflowAsync(workflowId, ct);
        var assignment = await BuildAssignmentAsync(req, ct);
        workflow.AddStage(req.Name, assignment, req.SlaHours);
        await _db.SaveChangesAsync(ct);
        return workflow.ToDto();
    }

    public async Task<WorkflowDto> UpdateStageAsync(int workflowId, int stageId, StageRequest req, CancellationToken ct)
    {
        var workflow = await RequireWorkflowAsync(workflowId, ct);
        var assignment = await BuildAssignmentAsync(req, ct);
        workflow.UpdateStage(stageId, req.Name, assignment, req.SlaHours);
        await _db.SaveChangesAsync(ct);
        return workflow.ToDto();
    }

    public async Task<WorkflowDto> RemoveStageAsync(int workflowId, int stageId, CancellationToken ct)
    {
        var workflow = await RequireWorkflowAsync(workflowId, ct);
        workflow.RemoveStage(stageId);
        await _db.SaveChangesAsync(ct);
        return workflow.ToDto();
    }

    public async Task<WorkflowDto> ReorderStagesAsync(int workflowId, ReorderStagesRequest req, CancellationToken ct)
    {
        var workflow = await RequireWorkflowAsync(workflowId, ct);
        workflow.Reorder(req.OrderedStageIds);
        await _db.SaveChangesAsync(ct);
        return workflow.ToDto();
    }

    public async Task<WorkflowDto> ActivateWorkflowAsync(int workflowId, CancellationToken ct)
    {
        var workflow = await RequireWorkflowAsync(workflowId, ct);

        // Enforce a single active workflow per document type. The filtered unique index would reject two
        // active rows even transiently, so deactivate others first (0 active) then activate the target
        // (1 active) as two saves inside one transaction.
        await _db.ExecuteInTransactionAsync(async token =>
        {
            var others = await _db.Workflows
                .Where(w => w.DocumentTypeId == workflow.DocumentTypeId && w.Id != workflow.Id && w.IsActive)
                .ToListAsync(token);
            foreach (var other in others) other.Deactivate();
            await _db.SaveChangesAsync(token);

            workflow.Activate();
            await _db.SaveChangesAsync(token);
        }, ct);

        return workflow.ToDto();
    }

    // ---- helpers ----

    private async Task<ApproverAssignment> BuildAssignmentAsync(StageRequest req, CancellationToken ct)
    {
        if (!Enum.TryParse<ApproverType>(req.ApproverType, ignoreCase: true, out var type))
            throw new ValidationException($"Unknown approver type '{req.ApproverType}'. Use Role, Department, or User.");

        switch (type)
        {
            case ApproverType.Role:
                if (req.RoleId is not int roleId) throw new ValidationException("RoleId is required for a Role stage.");
                if (!await _db.Roles.AnyAsync(r => r.Id == roleId, ct)) throw new NotFoundException($"Role {roleId} was not found.");
                return ApproverAssignment.ForRole(roleId);

            case ApproverType.Department:
                if (req.DepartmentId is not int deptId) throw new ValidationException("DepartmentId is required for a Department stage.");
                if (!await _db.Departments.AnyAsync(d => d.Id == deptId, ct)) throw new NotFoundException($"Department {deptId} was not found.");
                return ApproverAssignment.ForDepartment(deptId);

            case ApproverType.User:
                if (req.EmployeeId is not int empId) throw new ValidationException("EmployeeId is required for a User stage.");
                if (!await _db.Employees.AnyAsync(e => e.Id == empId, ct)) throw new NotFoundException($"Employee {empId} was not found.");
                return ApproverAssignment.ForUser(empId);

            default:
                throw new ValidationException($"Unsupported approver type '{req.ApproverType}'.");
        }
    }

    private async Task<Workflow> RequireWorkflowAsync(int id, CancellationToken ct)
        => await _db.Workflows.Include(w => w.Stages).FirstOrDefaultAsync(w => w.Id == id, ct)
           ?? throw new NotFoundException($"Workflow {id} was not found.");
}
