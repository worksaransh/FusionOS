using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.BusinessIntelligence.Application.KpiSnapshots.Contracts;
using MediatR;

namespace FusionOS.Modules.BusinessIntelligence.Application.KpiSnapshots.Queries.ListKpiSnapshots;

public sealed class ListKpiSnapshotsQueryHandler : IRequestHandler<ListKpiSnapshotsQuery, PagedResult<KpiSnapshotDto>>
{
    private readonly IKpiSnapshotRepository _repository;

    public ListKpiSnapshotsQueryHandler(IKpiSnapshotRepository repository) => _repository = repository;

    public async Task<PagedResult<KpiSnapshotDto>> Handle(ListKpiSnapshotsQuery request, CancellationToken cancellationToken)
    {
        var snapshots = await _repository.ListAsync(request.CompanyId, request.KpiDefinitionId, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, request.KpiDefinitionId, cancellationToken);

        var dtos = snapshots.Select(KpiSnapshotMapper.ToDto).ToList();

        return new PagedResult<KpiSnapshotDto>(dtos, request.Page, request.PageSize, total);
    }
}
