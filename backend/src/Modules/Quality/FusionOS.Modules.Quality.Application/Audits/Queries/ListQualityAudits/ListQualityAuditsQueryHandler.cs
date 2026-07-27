using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Quality.Application.Audits.Contracts;
using MediatR;

namespace FusionOS.Modules.Quality.Application.Audits.Queries.ListQualityAudits;

public sealed class ListQualityAuditsQueryHandler : IRequestHandler<ListQualityAuditsQuery, PagedResult<QualityAuditDto>>
{
    private readonly IQualityAuditRepository _repository;

    public ListQualityAuditsQueryHandler(IQualityAuditRepository repository) => _repository = repository;

    public async Task<PagedResult<QualityAuditDto>> Handle(ListQualityAuditsQuery request, CancellationToken cancellationToken)
    {
        var audits = await _repository.ListAsync(request.CompanyId, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, cancellationToken);

        var dtos = audits.Select(QualityAuditMapper.ToDto).ToList();

        return new PagedResult<QualityAuditDto>(dtos, request.Page, request.PageSize, total);
    }
}
