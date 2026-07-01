using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyProject.Core.Application.Common;
using MyProject.Core.Domain.Common;

namespace MyProject.Api.ExceptionHandling;

/// <summary>
/// Maps domain/application exceptions to RFC-7807 ProblemDetails responses. Keeps HTTP concerns out of
/// the services: they throw meaningful exceptions, this translates them to status codes.
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) => _logger = logger;

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (status, title, detail) = exception switch
        {
            ValidationException ex => (StatusCodes.Status400BadRequest, "Validation failed", ex.Message),
            DomainException ex => (StatusCodes.Status400BadRequest, "Business rule violated", ex.Message),
            NotFoundException ex => (StatusCodes.Status404NotFound, "Not found", ex.Message),
            ForbiddenException ex => (StatusCodes.Status403Forbidden, "Forbidden", ex.Message),
            ConflictException ex => (StatusCodes.Status409Conflict, "Conflict", ex.Message),
            DbUpdateConcurrencyException => (StatusCodes.Status409Conflict, "Concurrent update",
                "The record was modified by another user. Reload and try again."),
            _ => (StatusCodes.Status500InternalServerError, "Unexpected error", "An unexpected error occurred.")
        };

        if (status == StatusCodes.Status500InternalServerError)
            _logger.LogError(exception, "Unhandled exception.");

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };

        httpContext.Response.StatusCode = status;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }
}
