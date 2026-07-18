using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Manufacturing.Application.WorkOrders.Contracts;
using MediatR;

namespace FusionOS.Modules.Manufacturing.Application.WorkOrders.Queries.ListWorkOrders;

public sealed class ListWorkOrdersQueryHandler : IRequestHandler<ListWorkOrdersQuery, PagedResult<WorkOrderDto>>
{
    private readonly IWorkOrderRepository _repository;

    public ListWorkOrdersQueryHandler(IWorkOrderRepository repository) => _repository = repository;

    public async Task<PagedResult<WorkOrderDto>> Handle(ListWorkOrdersQuery request, CancellationToken cancellationToken)
    {
        var workOrders = await _repository.ListAsync(request.CompanyId, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, cancellationToken);

        var dtos = workOrders.Select(WorkOrderMapper.ToDto).ToList();

        return new PagedResult<WorkOrderDto>(dtos, request.Page, request.PageSize, total);
    }
}
