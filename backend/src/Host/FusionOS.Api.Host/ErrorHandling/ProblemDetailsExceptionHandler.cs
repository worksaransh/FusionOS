using FusionOS.BuildingBlocks.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Api.Host.ErrorHandling;

/// <summary>
/// Maps every unhandled exception to an RFC 7807 ProblemDetails response - the
/// single most serious API-layer gap flagged by the enterprise audit (08_API_STANDARDS.md
/// §6 documented this as already true; it was not). Registered via
/// builder.Services.AddExceptionHandler&lt;ProblemDetailsExceptionHandler&gt;() in
/// Program.cs, ahead of ASP.NET Core's own AddProblemDetails() fallback for
/// exception types this handler does not recognize.
///
/// Phase 10 audit fix (2026-07-27), two gaps found by three independent audits:
/// (1) every entity in this codebase uses UseXminAsConcurrencyToken(), but a
/// concurrent-write conflict (DbUpdateConcurrencyException) was never special-cased
/// here and fell through to the generic 500 branch - now mapped to 409 Conflict.
/// (2) InvalidOperationException's specific message (e.g. WorkOrder.RecordRework's
/// scrap-quantity-remaining check, JournalEntry.Post()'s state guard) was computed
/// by the domain but then discarded - only a generic title reached the client. Both
/// InvalidOperationException and DbUpdateConcurrencyException now surface
/// exception.Message via ProblemDetails.Detail so the specific business-rule reason
/// isn't silently dropped.
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
            // Added in the 2026-07-14 coverage audit follow-up: InvalidOperationException is this
            // codebase's existing convention for domain-state-invariant violations (see
            // PurchaseOrder.Approve()'s "only Draft can be approved" check, JournalEntry.Post()'s
            // "only Draft can be posted" check, and ApprovePurchaseOrderCommandHandler's
            // self-approval rejection) - all of these were previously falling through to a bare
            // 500 instead of a real 409 Conflict. KeyNotFoundException is this codebase's existing
            // convention for "the referenced entity does not exist" (see AssignUserRoleCommandHandler,
            // GetRolePermissionsQueryHandler) - previously also a bare 500 instead of 404.
            InvalidOperationException => (StatusCodes.Status409Conflict, "The request conflicts with the current state of the resource."),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "The requested resource was not found."),
            // Phase 10 audit fix: every entity uses xmin optimistic concurrency, so a stale
            // read-then-write (two admins editing the same record) must surface as a clean
            // 409, not an opaque 500.
            DbUpdateConcurrencyException => (StatusCodes.Status409Conflict, "This record was modified by someone else. Reload and try again."),
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

        // Phase 10 audit fix: surface the domain's own specific message instead of only
        // the generic per-status-code title - e.g. "Cannot record rework: only 3 units remain
        // unaccounted for" instead of just "The request conflicts with the current state...".
        if (exception is InvalidOperationException)
            problemDetails.Detail = exception.Message;

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
