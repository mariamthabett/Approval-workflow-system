# Generic Approval Workflow Engine (ASP.NET Core .NET 10)

A reusable, document-agnostic approval engine for an ERP. The engine owns workflows, stages, runtime
approval instances, and an immutable audit trail; each business document (LeaveRequest, Purchase
Request, …) plugs in via a loose `(DocumentType, DocumentId)` reference and reacts to engine events —
so **adding a new document type requires no change to the approval logic**.

See the full design (architecture, ERD, sequence diagrams, state machine, security, scalability) in
`../.claude/plans/act-as-a-senior-purrfect-perlis.md`.

## Solution layout (Clean Architecture / DDD)

```
ApprovalWorkflow.slnx
├─ MyProject.Core         # class library
│  ├─ Domain/             # entities, value objects, enums, the ApprovalInstance state machine
│  ├─ Application/        # use-case services, DTOs, abstractions (ICurrentUser, IApproverResolver, …)
│  └─ Infrastructure/     # EF Core DbContext + configs + migration, resolver, outbox handlers, seed
└─ MyProject              # ASP.NET Core host (minimal-API endpoints, JWT auth, outbox dispatcher, DI)
```

Dependency direction is inward: `MyProject → MyProject.Core`, and inside Core `Infrastructure → Application → Domain`. `Domain` has no framework dependencies.

## Running it

The app **applies the schema and seeds demo data on startup**, then listens on `http://localhost:5294`.

### Development (SQLite — zero install, the default in `appsettings.Development.json`)

```bash
dotnet run --project MyProject
```

Creates a local `approvalworkflow.db` SQLite file via `EnsureCreated`. No database server required.

### Production (SQL Server)

Set the provider to SQL Server and point the connection string at your instance:

- `appsettings.json` → `"Database": { "Provider": "SqlServer" }` (already the base default)
- `ConnectionStrings:Default` → your SQL Server
- The app runs EF migrations (`InitialCreate`) on startup. To manage them manually:
  ```bash
  dotnet ef database update --project MyProject.Core --startup-project MyProject
  ```

Provider selection is config-driven (`Database:Provider` = `SqlServer` | `Sqlite`). Only two behaviors
are SQL-Server-only: the `rowversion` optimistic-concurrency `409` and the filtered-index quoting.

## Seeded users (dev login stub)

`POST /api/auth/login { "email": "<one below>" }` returns a JWT. No password — it is a local stub to be
replaced by real auth/SSO (the engine only reads the `employeeId` / `role` / `departmentId` claims).

| Email | Role / position | Where they act in the seeded LeaveRequest workflow |
|---|---|---|
| `alice@example.com` | Employee | initiator (creates & submits leave) |
| `bob@example.com` | Engineering **manager** | Stage 1 "Manager Approval" (Department approver type) |
| `carol@example.com` | **DeptHead** role | Stage 2 "Department Head" (Role approver type) |
| `dan@example.com` | HR | Stage 3 "HR Approval" (User approver type) |
| `admin@example.com` | **WorkflowAdmin** | workflow configuration endpoints |

## Try the flow

Open `MyProject/MyProject.http` (VS / VS Code REST Client) and run top-to-bottom: login → create leave →
submit → approve through all three stages → history; then a reject → edit → resubmit cycle; then
dashboards and admin endpoints. Endpoints are summarized in the design doc (§8).

## Key endpoints

- **Auth**: `POST /api/auth/login`, `GET /api/auth/whoami`
- **Generic approvals**: `POST /api/approvals/submit`, `/{id}/approve|reject|comment|resubmit|cancel`, `GET /{id}`, `GET /{id}/history`
- **Sample module**: `POST /api/leave-requests`, `PUT /{id}`, `POST /{id}/submit`
- **Dashboards**: `GET /api/dashboard/my-pending|my-documents|sla-breaches`, `/workflows/{id}/metrics`
- **Admin (WorkflowAdmin)**: `/api/document-types`, `/api/workflows` (+ stages CRUD, `/stages/reorder`, `/activate`)

## Notes

- **Immutable audit**: `ApprovalAction` rows are append-only, enforced by `AuditImmutabilityInterceptor`
  (and, on SQL Server, intended to be backed by a DB principal denied UPDATE/DELETE).
- **Outbox**: state changes enqueue integration events; `OutboxDispatcher` (a hosted service) delivers
  them to subscribers — notifications and the LeaveRequest lock/unlock handler — with retry.
- **Known advisory**: `Microsoft.OpenApi` 2.0.0 (a transitive dependency of `Microsoft.AspNetCore.OpenApi`
  10.0.9) has advisory NU1903/GHSA-v5pm-xwqc-g5wc. The only newer releases are 3.x (a breaking major the
  ASP.NET package does not accept), so this clears when Microsoft ships a patched `Microsoft.AspNetCore.OpenApi`.
  The OpenAPI document endpoint is only mapped in the Development environment.
```
