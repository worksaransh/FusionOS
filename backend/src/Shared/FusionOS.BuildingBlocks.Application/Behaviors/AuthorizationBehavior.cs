using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.SharedKernel.Context;
using MediatR;

namespace FusionOS.BuildingBlocks.Application.Behaviors;

/// <summary>
/// Enforces RBAC permission checks server-side — the actual security boundary
/// per 07_SECURITY.md §2. The frontend permission-aware UI (06_UI_UX_DESIGN_SYSTEM.md
/// §8) is a UX layer only; this behavior is what actually protects the data.
/// </summary>
public sealed class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ICurrentUserContext _currentUser;

    public AuthorizationBehavior(ICurrentUserContext currentUser) => _currentUser = currentUser;

    public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is IRequirePermission requirement)
        {
            foreach (var permission in requirement.RequiredPermissions)
            {
                if (!_currentUser.HasPermission(permission))
                    throw new ForbiddenException(permission);
            }
        }

        return next();
    }
}
