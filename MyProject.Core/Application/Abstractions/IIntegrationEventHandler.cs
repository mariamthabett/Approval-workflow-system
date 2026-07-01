using MyProject.Core.Application.Approvals;

namespace MyProject.Core.Application.Abstractions;

/// <summary>
/// A subscriber to approval integration events delivered from the outbox. Modules (e.g. LeaveRequest)
/// and cross-cutting concerns (notifications) implement this to react to engine state changes without
/// the engine referencing them. Handlers mutate the shared scoped <see cref="IAppDbContext"/>; the
/// dispatcher commits the unit of work.
/// </summary>
public interface IIntegrationEventHandler
{
    bool CanHandle(string eventType, ApprovalEventPayload payload);
    Task HandleAsync(ApprovalEventPayload payload, CancellationToken cancellationToken = default);
}
