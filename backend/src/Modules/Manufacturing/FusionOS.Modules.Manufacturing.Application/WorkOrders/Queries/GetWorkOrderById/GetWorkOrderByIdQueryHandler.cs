using FusionOS.Modules.Manufacturing.Application.WorkOrders.Contracts;
using MediatR;

namespace FusionOS.Modules.Manufacturing.Application.WorkOrders.Queries.GetWorkOrderById;

public sealed class GetWorkOrderByIdQueryHandler : IRequestHandler<GetWorkOrderByIdQuery, WorkOrderDto>
{
    private readonly IWorkOrderRepository _repository;

    public GetWorkOrderByIdQueryHandler(IWorkOrderRepository repository) => _repository = repository;

    public async Task<WorkOrderDto> Handle(GetWorkOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var workOrder = await _repository.GetByIdAsync(request.CompanyId, request.WorkOrderId, cancellationToken)
            ?? throw new KeyNotFoundException($"Work order '{request.WorkOrderId}' was not found.");

        return WorkOrderMapper.ToDto(workOrder);
    }
}
