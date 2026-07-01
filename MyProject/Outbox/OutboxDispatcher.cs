using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MyProject.Core.Application.Abstractions;
using MyProject.Core.Application.Approvals;
using MyProject.Core.Infrastructure.Persistence;

namespace MyProject.Api.Outbox;

/// <summary>
/// Background dispatcher for the transactional outbox. Polls unprocessed messages and delivers each to
/// the matching <see cref="IIntegrationEventHandler"/>s in an isolated scope, so a failing message is
/// retried without blocking or corrupting others. Handler mutations + the processed-mark commit together.
/// </summary>
public sealed class OutboxDispatcher : BackgroundService
{
    private static readonly TimeSpan IdleDelay = TimeSpan.FromSeconds(2);
    private const int BatchSize = 20;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxDispatcher> _logger;

    public OutboxDispatcher(IServiceScopeFactory scopeFactory, ILogger<OutboxDispatcher> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            int processed = 0;
            try
            {
                processed = await ProcessBatchAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Outbox batch failed.");
            }

            if (processed == 0)
            {
                try { await Task.Delay(IdleDelay, stoppingToken); }
                catch (TaskCanceledException) { break; }
            }
        }
    }

    private async Task<int> ProcessBatchAsync(CancellationToken ct)
    {
        List<long> ids;
        using (var scope = _scopeFactory.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            ids = await db.OutboxMessages
                .Where(m => m.ProcessedAtUtc == null)
                .OrderBy(m => m.Id)
                .Take(BatchSize)
                .Select(m => m.Id)
                .ToListAsync(ct);
        }

        foreach (var id in ids)
            await ProcessOneAsync(id, ct);

        return ids.Count;
    }

    private async Task ProcessOneAsync(long messageId, CancellationToken ct)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var message = await db.OutboxMessages.FirstOrDefaultAsync(m => m.Id == messageId, ct);
            if (message is null || message.ProcessedAtUtc is not null) return;

            var payload = JsonSerializer.Deserialize<ApprovalEventPayload>(message.PayloadJson)
                ?? throw new InvalidOperationException($"Outbox message {messageId} has an invalid payload.");

            foreach (var handler in scope.ServiceProvider.GetServices<IIntegrationEventHandler>())
                if (handler.CanHandle(message.EventType, payload))
                    await handler.HandleAsync(payload, ct);

            message.MarkProcessed(DateTime.UtcNow);
            await db.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Outbox message {MessageId} failed; recording failure.", messageId);
            await RecordFailureAsync(messageId, ex, ct);
        }
    }

    private async Task RecordFailureAsync(long messageId, Exception ex, CancellationToken ct)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var message = await db.OutboxMessages.FirstOrDefaultAsync(m => m.Id == messageId, ct);
            if (message is null) return;
            message.MarkFailed(ex.Message);
            await db.SaveChangesAsync(ct);
        }
        catch (Exception inner)
        {
            _logger.LogError(inner, "Failed to record outbox failure for message {MessageId}.", messageId);
        }
    }
}
