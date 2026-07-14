using System.Collections.Concurrent;
using System.Reflection;
using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.SharedKernel.Context;
using MediatR;

namespace FusionOS.BuildingBlocks.Application.Behaviors;

/// <summary>
/// Closes the tenant-isolation gap flagged by the enterprise audit: EF's global
/// query filters only ever excluded soft-deleted rows, never scoped queries to
/// the caller's own CompanyId. Any command/query with a non-nullable Guid
/// "CompanyId" property is checked here, reflectively, against the JWT's
/// company_id claim (ICurrentUserContext.CompanyId) - one central chokepoint
/// rather than trusting every individual handler to remember the check.
/// Requests with no CompanyId property (e.g. CreateCompanyCommand, the tenant
/// bootstrap action, or Login before a company is even selected) pass through
/// untouched.
/// </summary>
public sealed class TenantIsolationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo?> CompanyIdProperties = new();

    private readonly ICurrentUserContext _currentUser;

    public TenantIsolationBehavior(ICurrentUserContext currentUser) => _currentUser = currentUser;

    public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var property = CompanyIdProperties.GetOrAdd(typeof(TRequest), FindCompanyIdProperty);

        if (property is not null)
        {
            var requestCompanyId = (Guid)property.GetValue(request)!;
            if (_currentUser.CompanyId is not { } currentCompanyId || requestCompanyId != currentCompanyId)
                throw new ForbiddenException($"company-scope:{requestCompanyId}");
        }

        return next();
    }

    private static PropertyInfo? FindCompanyIdProperty(Type requestType)
    {
        var property = requestType.GetProperty("CompanyId");
        return property is not null && property.PropertyType == typeof(Guid) ? property : null;
    }
}
