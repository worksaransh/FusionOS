namespace FusionOS.BuildingBlocks.Application.Exceptions;

/// <summary>Thrown by AuthorizationBehavior when the current user lacks a required permission.</summary>
public sealed class ForbiddenException : Exception
{
    public ForbiddenException(string permissionCode)
        : base($"The current user does not have the '{permissionCode}' permission.") { }

    private ForbiddenException(string message, bool isPreformattedMessage) : base(message) { }

    /// <summary>
    /// Same exception type/HTTP mapping as the permission-denial constructor above (see
    /// ProblemDetailsExceptionHandler — it maps by type, not message text), so callers get the
    /// same already-tested 403 shape, but with a caller-supplied message instead of the fixed
    /// "does not have the '{permissionCode}' permission" template. Used by
    /// PlanEntitlementBehavior, whose denial reason ("plan does not include module X") isn't a
    /// missing-permission story at all.
    /// </summary>
    public static ForbiddenException WithMessage(string message) => new(message, isPreformattedMessage: true);
}
