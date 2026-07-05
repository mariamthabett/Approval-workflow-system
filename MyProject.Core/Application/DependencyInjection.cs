using Microsoft.Extensions.DependencyInjection;
using MyProject.Core.Application.Approvals;
using MyProject.Core.Application.Auditing;
using MyProject.Core.Application.Auth;
using MyProject.Core.Application.Dashboards;
using MyProject.Core.Application.Documents;
using MyProject.Core.Application.Workflows;

namespace MyProject.Core.Application;

public static class DependencyInjection
{
    /// <summary>Registers the application use-case services (scoped per request).</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<AuthAppService>();
        services.AddScoped<ActivityQueryService>();
        services.AddScoped<WorkflowAdminService>();
        services.AddScoped<ApprovalAppService>();
        services.AddScoped<DashboardQueryService>();
        services.AddScoped<LeaveRequestService>();
        return services;
    }
}
