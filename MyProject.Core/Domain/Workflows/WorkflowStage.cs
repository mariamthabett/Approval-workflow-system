using MyProject.Core.Domain.Enums;

namespace MyProject.Core.Domain.Workflows;

/// <summary>
/// One ordered step of a workflow template. Part of the <see cref="Workflow"/> aggregate — never
/// modified directly by application code; go through the aggregate root.
/// </summary>
public sealed class WorkflowStage
{
    public int Id { get; private set; }
    public int WorkflowId { get; private set; }
    public int StageOrder { get; private set; }
    public string Name { get; private set; } = string.Empty;

    public ApproverType ApproverType { get; private set; }
    public int? ApproverRoleId { get; private set; }
    public int? ApproverDepartmentId { get; private set; }
    public int? ApproverEmployeeId { get; private set; }

    public int? SlaHours { get; private set; }
    public bool IsActive { get; private set; } = true;

    private WorkflowStage() { }

    internal WorkflowStage(int stageOrder, string name, ApproverAssignment approver, int? slaHours)
    {
        StageOrder = stageOrder;
        Name = name;
        SlaHours = slaHours;
        Apply(approver);
    }

    public ApproverAssignment Approver
        => ApproverAssignment.FromColumns(ApproverType, ApproverRoleId, ApproverDepartmentId, ApproverEmployeeId);

    internal void SetOrder(int order) => StageOrder = order;

    internal void Update(string name, ApproverAssignment approver, int? slaHours)
    {
        Name = name;
        SlaHours = slaHours;
        Apply(approver);
    }

    private void Apply(ApproverAssignment approver)
    {
        ApproverType = approver.Type;
        ApproverRoleId = approver.RoleId;
        ApproverDepartmentId = approver.DepartmentId;
        ApproverEmployeeId = approver.EmployeeId;
    }
}
