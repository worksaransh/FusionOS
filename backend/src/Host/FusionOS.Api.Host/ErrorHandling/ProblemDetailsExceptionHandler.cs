using FusionOS.BuildingBlocks.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Api.Host.ErrorHandling;

/// <summary>
/// Maps every unhandled exception to an RFC 7807 ProblemDetails response - the
/// single most serious API-layer gap flagged by the enterprise audit (08_API_STANDARDS.md
/// §6 documented this as already true; it was not). Registered via
/// builder.Services.AddExceptionHandler&lt;ProblemDetailsExceptionHandler&gt;() in
/// Program.cs, ahead of ASP.NET Core's own AddProblemDetails() fallback for
/// exception types this handler does not recognize.
/// </summary>
public sealed class ProblemDetailsExceptionHandler : IExceptionHandler
{
    private readonly ILogger<ProblemDetailsExceptionHandler> _logger;

    public ProblemDetailsExceptionHandler(ILogger<ProblemDetailsExceptionHandler> logger) => _logger = logger;

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (statusCode, title) = exception switch
        {
            ValidationException => (StatusCodes.Status400BadRequest, "One or more validation errors occurred."),
            ForbiddenException => (StatusCodes.Status403Forbidden, "You do not have permission to perform this action."),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred."),
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
            _logger.LogError(exception, "Unhandled exception processing {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Type = $"https://httpstatuses.io/{statusCode}",
            Instance = httpContext.Request.Path,
        };
        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

        if (exception is ValidationException validationException)
            problemDetails.Extensions["errors"] = validationException.Errors;

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
