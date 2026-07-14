using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.SharedKernel.Auditing;
using FusionOS.SharedKernel.Context;
using MediatR;

namespace FusionOS.BuildingBlocks.Application.Behaviors;

/// <summary>Writes an audit log entry after a successful IAuditableCommand — 04_DATABASE_GUIDELINES.md §5.</summary>
public sealed class AuditBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IAuditLogWriter _auditLogWriter;
    private readonly ICurrentUserContext _currentUser;

    public AuditBehavior(IAuditLogWriter auditLogWriter, ICurrentUserContext currentUser)
    {
        _auditLogWriter = auditLogWriter;
        _currentUser = currentUser;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var response = await next();

        if (request is IAuditableCommand auditable && _currentUser.CompanyId is not null && _currentUser.UserId is not null)
        {
            // The 12 Create* commands can't know their new entity's id when the
            // command object is constructed (audit fix, ex-audit finding: every
            // Create command's EntityId was silently logged as Guid.Empty).
            // Every DTO in this codebase names its identifier property "Id"
            // (see e.g. CompanyDto), so read it back off the handler's response
            // instead of threading an out-parameter through every handler.
            var entityId = auditable.EntityId != Guid.Empty
                ? auditable.EntityId
                : TryReadIdFromResponse(response) ?? Guid.Empty;

            await _auditLogWriter.WriteAsync(new AuditLogEntry(
                Id: Guid.NewGuid(),
                EntityType: auditable.EntityType,
                EntityId: entityId,
                Action: auditable.Action,
                ActorId: _currentUser.UserId.Value,
                CompanyId: _currentUser.CompanyId.Value,
                BranchId: _currentUser.BranchId,
                OccurredAt: DateTimeOffset.UtcNow,
                ChangesJson: null,
                CorrelationId: _currentUser.CorrelationId), cancellationToken);
        }

        return response;
    }

    private static Guid? TryReadIdFromResponse(TResponse response)
    {
        if (response is null) return null;

        var property = response.GetType().GetProperty("Id");
        if (property is null || property.PropertyType != typeof(Guid)) return null;

        return (Guid)property.GetValue(response)!;
    }
}
