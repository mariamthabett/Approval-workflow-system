using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyProject.Core.Application.Abstractions;
using MyProject.Core.Infrastructure.Approvals;
using MyProject.Core.Infrastructure.Auditing;
using MyProject.Core.Infrastructure.Notifications;
using MyProject.Core.Infrastructure.Outbox;
using MyProject.Core.Infrastructure.Persistence;
using MyProject.Core.Infrastructure.Security;
using MyProject.Core.Infrastructure.Time;

namespace MyProject.Core.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registers persistence and infrastructure services. The host supplies the provider
    /// ("SqlServer" [default] or "Sqlite") and connection string.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string provider, string connectionString)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            if (string.Equals(provider, "Sqlite", StringComparison.OrdinalIgnoreCase))
                options.UseSqlite(connectionString);
            else
                options.UseSqlServer(connectionString);

            options.AddInterceptors(new AuditImmutabilityInterceptor());
        });
        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddScoped<IActivityLogger, ActivityLogger>();
        services.AddScoped<IApproverResolver, ApproverResolver>();
        services.AddScoped<INotificationSender, InAppNotificationSender>();

        // Outbox subscribers — add a new IIntegrationEventHandler per module; the engine is untouched.
        services.AddScoped<IIntegrationEventHandler, NotificationIntegrationHandler>();
        services.AddScoped<IIntegrationEventHandler, LeaveRequestLockHandler>();

        return services;
    }
}
