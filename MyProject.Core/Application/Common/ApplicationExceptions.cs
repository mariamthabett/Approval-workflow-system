namespace MyProject.Core.Application.Common;

/// <summary>Requested resource does not exist. Mapped to HTTP 404.</summary>
public sealed class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

/// <summary>Caller is authenticated but not permitted (e.g. not the current approver). Mapped to HTTP 403.</summary>
public sealed class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message) { }
}

/// <summary>State conflict — duplicate active instance, or optimistic-concurrency clash. Mapped to HTTP 409.</summary>
public sealed class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}

/// <summary>Input validation failure. Mapped to HTTP 400.</summary>
public sealed class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
}
