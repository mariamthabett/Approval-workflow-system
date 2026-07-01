namespace MyProject.Core.Domain.Common;

/// <summary>
/// Thrown when a domain invariant / state-machine rule is violated
/// (e.g. approving a stage that is not the current one). Mapped to HTTP 400.
/// </summary>
public sealed class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

/// <summary>Marker for aggregate roots — the only entities repositories load/save directly.</summary>
public interface IAggregateRoot { }
