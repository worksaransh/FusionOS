using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Core.Application.AuditLog.Contracts;
using MediatR;

namespace FusionOS.Modules.Core.Application.AuditLog.Queries.ListAuditLogEntries;

public sealed class ListAuditLogEntriesQueryHandler : IRequestHandler<ListAuditLogEntriesQuery, PagedResult<AuditLogEntryDto>>
{
    private readonly IAuditLogRepository _repository;

    public ListAuditLogEntriesQueryHandler(IAuditLogRepository repository) => _repository = repository;

    public async Task<PagedResult<AuditLogEntryDto>> Handle(ListAuditLogEntriesQuery request, CancellationToken cancellationToken)
    {
        var entries = await _repository.ListAsync(request.CompanyId, request.Search, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, request.Search, cancellationToken);

        return new PagedResult<AuditLogEntryDto>(entries, request.Page, request.PageSize, total);
    }
}
