using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using MyProject.Core.Infrastructure.Persistence;

namespace MyProject.Api.Persistence;

/// <summary>
/// Design-time factory so <c>dotnet ef</c> can build the model without spinning up the whole host.
/// Uses the APPROVALWORKFLOW_CONNECTION env var when present, else a LocalDB default.
/// Run: <c>dotnet ef migrations add &lt;Name&gt; --project MyProject.Core --startup-project MyProject</c>.
/// </summary>
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("APPROVALWORKFLOW_CONNECTION")
            ?? "Server=(localdb)\\MSSQLLocalDB;Database=ApprovalWorkflow;Trusted_Connection=True;TrustServerCertificate=True;";

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new AppDbContext(options);
    }
}
