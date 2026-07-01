using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyProject.Core.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApprovalInstances",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentTypeId = table.Column<int>(type: "int", nullable: false),
                    DocumentId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    WorkflowId = table.Column<int>(type: "int", nullable: false),
                    WorkflowVersion = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CycleNumber = table.Column<int>(type: "int", nullable: false),
                    CurrentStageOrder = table.Column<int>(type: "int", nullable: true),
                    InitiatorEmployeeId = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalInstances", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DocumentTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LeaveRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OwnerEmployeeId = table.Column<int>(type: "int", nullable: false),
                    FromDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ToDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsLocked = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecipientEmployeeId = table.Column<int>(type: "int", nullable: false),
                    ApprovalInstanceId = table.Column<long>(type: "bigint", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApprovalInstanceId = table.Column<long>(type: "bigint", nullable: true),
                    EventType = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    PayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OccurredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Attempts = table.Column<int>(type: "int", nullable: false),
                    Error = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApprovalActions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApprovalInstanceId = table.Column<long>(type: "bigint", nullable: false),
                    StageInstanceId = table.Column<long>(type: "bigint", nullable: true),
                    CycleNumber = table.Column<int>(type: "int", nullable: false),
                    ActionType = table.Column<int>(type: "int", nullable: false),
                    ActedByEmployeeId = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    FromStatus = table.Column<int>(type: "int", nullable: true),
                    ToStatus = table.Column<int>(type: "int", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApprovalActions_ApprovalInstances_ApprovalInstanceId",
                        column: x => x.ApprovalInstanceId,
                        principalTable: "ApprovalInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StageInstances",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApprovalInstanceId = table.Column<long>(type: "bigint", nullable: false),
                    WorkflowStageId = table.Column<int>(type: "int", nullable: true),
                    CycleNumber = table.Column<int>(type: "int", nullable: false),
                    StageOrder = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ApproverType = table.Column<int>(type: "int", nullable: false),
                    ResolvedApproverEmployeeId = table.Column<int>(type: "int", nullable: true),
                    ApproverRoleId = table.Column<int>(type: "int", nullable: true),
                    ApproverDepartmentId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ActedByEmployeeId = table.Column<int>(type: "int", nullable: true),
                    ActedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EnteredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SlaHours = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StageInstances", x => x.Id);
                    table.CheckConstraint("CK_StageInstance_Approver", "([ApproverType] = 1 AND [ApproverRoleId] IS NOT NULL) OR ([ApproverType] = 2 AND [ApproverDepartmentId] IS NOT NULL) OR ([ApproverType] = 3 AND [ResolvedApproverEmployeeId] IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_StageInstances_ApprovalInstances_ApprovalInstanceId",
                        column: x => x.ApprovalInstanceId,
                        principalTable: "ApprovalInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Workflows",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentTypeId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedByEmployeeId = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workflows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Workflows_DocumentTypes_DocumentTypeId",
                        column: x => x.DocumentTypeId,
                        principalTable: "DocumentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WorkflowStages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkflowId = table.Column<int>(type: "int", nullable: false),
                    StageOrder = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ApproverType = table.Column<int>(type: "int", nullable: false),
                    ApproverRoleId = table.Column<int>(type: "int", nullable: true),
                    ApproverDepartmentId = table.Column<int>(type: "int", nullable: true),
                    ApproverEmployeeId = table.Column<int>(type: "int", nullable: true),
                    SlaHours = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkflowStages", x => x.Id);
                    table.CheckConstraint("CK_WorkflowStage_Approver", "([ApproverType] = 1 AND [ApproverRoleId] IS NOT NULL AND [ApproverDepartmentId] IS NULL AND [ApproverEmployeeId] IS NULL) OR ([ApproverType] = 2 AND [ApproverDepartmentId] IS NOT NULL AND [ApproverRoleId] IS NULL AND [ApproverEmployeeId] IS NULL) OR ([ApproverType] = 3 AND [ApproverEmployeeId] IS NOT NULL AND [ApproverRoleId] IS NULL AND [ApproverDepartmentId] IS NULL)");
                    table.ForeignKey(
                        name: "FK_WorkflowStages_Workflows_WorkflowId",
                        column: x => x.WorkflowId,
                        principalTable: "Workflows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ManagerEmployeeId = table.Column<int>(type: "int", nullable: true),
                    ParentDepartmentId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Departments_Departments_ParentDepartmentId",
                        column: x => x.ParentDepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Employees_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeRoles",
                columns: table => new
                {
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeRoles", x => new { x.EmployeeId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_EmployeeRoles_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeeRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalActions_ApprovalInstanceId_CreatedAtUtc",
                table: "ApprovalActions",
                columns: new[] { "ApprovalInstanceId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalInstances_DocumentTypeId_DocumentId",
                table: "ApprovalInstances",
                columns: new[] { "DocumentTypeId", "DocumentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalInstances_InitiatorEmployeeId_Status",
                table: "ApprovalInstances",
                columns: new[] { "InitiatorEmployeeId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Departments_ManagerEmployeeId",
                table: "Departments",
                column: "ManagerEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_ParentDepartmentId",
                table: "Departments",
                column: "ParentDepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTypes_Code",
                table: "DocumentTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeRoles_RoleId",
                table: "EmployeeRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_DepartmentId",
                table: "Employees",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_Email",
                table: "Employees",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeaveRequests_OwnerEmployeeId",
                table: "LeaveRequests",
                column: "OwnerEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RecipientEmployeeId_IsRead",
                table: "Notifications",
                columns: new[] { "RecipientEmployeeId", "IsRead" });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_ProcessedAtUtc",
                table: "OutboxMessages",
                column: "ProcessedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Code",
                table: "Roles",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StageInstances_ApprovalInstanceId_Status",
                table: "StageInstances",
                columns: new[] { "ApprovalInstanceId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Workflows_DocumentTypeId_Version",
                table: "Workflows",
                columns: new[] { "DocumentTypeId", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Workflow_ActivePerDocumentType",
                table: "Workflows",
                column: "DocumentTypeId",
                unique: true,
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_WorkflowStages_WorkflowId_StageOrder",
                table: "WorkflowStages",
                columns: new[] { "WorkflowId", "StageOrder" });

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Employees_ManagerEmployeeId",
                table: "Departments",
                column: "ManagerEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Employees_ManagerEmployeeId",
                table: "Departments");

            migrationBuilder.DropTable(
                name: "ApprovalActions");

            migrationBuilder.DropTable(
                name: "EmployeeRoles");

            migrationBuilder.DropTable(
                name: "LeaveRequests");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "OutboxMessages");

            migrationBuilder.DropTable(
                name: "StageInstances");

            migrationBuilder.DropTable(
                name: "WorkflowStages");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "ApprovalInstances");

            migrationBuilder.DropTable(
                name: "Workflows");

            migrationBuilder.DropTable(
                name: "DocumentTypes");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "Departments");
        }
    }
}
