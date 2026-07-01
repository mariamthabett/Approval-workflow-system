namespace MyProject.Core.Application.Abstractions;

/// <summary>Abstracts the current time so domain flows are deterministic and testable.</summary>
public interface IClock
{
    DateTime UtcNow { get; }
}
