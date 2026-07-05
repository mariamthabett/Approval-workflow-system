// DTO shapes returned by the ASP.NET Core API (camelCase JSON).

export interface Session { token: string; me: number; roles: string[]; name: string; email: string; }

export interface RefEmployee { id: number; fullName: string; email: string; departmentId: number; }
export interface RefRole { id: number; code: string; name: string; }
export interface RefDept { id: number; name: string; managerEmployeeId: number | null; }

export interface PendingApproval {
  instanceId: number; documentTypeId: number; documentTypeCode: string; documentId: string;
  cycleNumber: number; stageOrder: number; stageName: string;
  initiatorEmployeeId: number; enteredAtUtc: string; dueAtUtc: string | null; isOverdue: boolean;
}

export interface MyDocument {
  instanceId: number; documentTypeId: number; documentTypeCode: string; documentId: string;
  status: string; cycleNumber: number; currentStageOrder: number | null; currentStageName: string | null;
  createdAtUtc: string; completedAtUtc: string | null;
}

export interface StageInstance {
  id: number; cycleNumber: number; stageOrder: number; name: string; approverType: string;
  status: string; actedByEmployeeId: number | null; actedAtUtc: string | null;
  enteredAtUtc: string; dueAtUtc: string | null;
}

export interface ApprovalInstance {
  id: number; documentTypeId: number; documentId: string; workflowId: number; workflowVersion: number;
  status: string; cycleNumber: number; currentStageOrder: number | null; currentStageName: string | null;
  initiatorEmployeeId: number; createdAtUtc: string; completedAtUtc: string | null; stages: StageInstance[];
}

export interface ApprovalAction {
  id: number; cycleNumber: number; actionType: string; actedByEmployeeId: number;
  comment: string | null; fromStatus: string | null; toStatus: string | null; createdAtUtc: string;
}

export interface LeaveRequest {
  id: number; ownerEmployeeId: number; fromDate: string; toDate: string; reason: string;
  status: string; isLocked: boolean; createdAtUtc: string;
}

export interface DocumentType { id: number; code: string; name: string; isActive: boolean; }

export interface WorkflowStage {
  id: number; stageOrder: number; name: string; approverType: string;
  approverRoleId: number | null; approverDepartmentId: number | null; approverEmployeeId: number | null; slaHours: number | null;
}

export interface Workflow {
  id: number; documentTypeId: number; name: string; version: number; isActive: boolean; stages: WorkflowStage[];
}

export interface WorkflowMetrics {
  workflowId: number; total: number; pending: number; approved: number; rejected: number; cancelled: number;
  averageCycleTimeHours: number | null;
}
