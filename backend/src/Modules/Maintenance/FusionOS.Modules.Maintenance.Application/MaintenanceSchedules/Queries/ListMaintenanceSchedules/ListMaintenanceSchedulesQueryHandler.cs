using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Maintenance.Application.MaintenanceSchedules.Contracts;
using MediatR;

namespace FusionOS.Modules.Maintenance.Application.MaintenanceSchedules.Queries.ListMaintenanceSchedules;

public sealed class ListMaintenanceSchedulesQueryHandler : IRequestHandler<ListMaintenanceSchedulesQuery, PagedResult<MaintenanceScheduleDto>>
{
    private readonly IMaintenanceScheduleRepository _repository;

    public ListMaintenanceSchedulesQueryHandler(IMaintenanceScheduleRepository repository) => _repository = repository;

    public async Task<PagedResult<MaintenanceScheduleDto>> Handle(ListMaintenanceSchedulesQuery request, CancellationToken cancellationToken)
    {
        var schedules = await _repository.ListAsync(request.CompanyId, request.AssetId, request.DueFilter, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, request.AssetId, request.DueFilter, cancellationToken);

        var dtos = schedules.Select(MaintenanceScheduleMapper.ToDto).ToList();

        return new PagedResult<MaintenanceScheduleDto>(dtos, request.Page, request.PageSize, total);
    }
}
