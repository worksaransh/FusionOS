using FusionOS.Modules.Maintenance.Application.MaintenanceRequests.Contracts;
using MediatR;

namespace FusionOS.Modules.Maintenance.Application.MaintenanceRequests.Queries.ListMaintenanceRequests;

public sealed class ListMaintenanceRequestsQueryHandler : IRequestHandler<ListMaintenanceRequestsQuery, PagedResult<MaintenanceRequestDto>>
{
    private readonly IMaintenanceRequestRepository _repository;

    public ListMaintenanceRequestsQueryHandler(IMaintenanceRequestRepository repository) => _repository = repository;

    public async Task<PagedResult<MaintenanceRequestDto>> Handle(ListMaintenanceRequestsQuery request, CancellationToken cancellationToken)
    {
        var requests = await _repository.ListAsync(request.CompanyId, request.AssetId, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, request.AssetId, cancellationToken);

        var dtos = requests.Select(MaintenanceRequestMapper.ToDto).ToList();

        return new PagedResult<MaintenanceRequestDto>(dtos, request.Page, request.PageSize, total);
    }
}
