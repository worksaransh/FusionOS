namespace FusionOS.BuildingBlocks.Application.Exceptions;

/// <summary>Thrown by AuthorizationBehavior when the current user lacks a required permission.</summary>
public sealed class ForbiddenException : Exception
{
    public ForbiddenException(string permissionCode)
        : base($"The current user does not have the '{permissionCode}' permission.") { }
}
