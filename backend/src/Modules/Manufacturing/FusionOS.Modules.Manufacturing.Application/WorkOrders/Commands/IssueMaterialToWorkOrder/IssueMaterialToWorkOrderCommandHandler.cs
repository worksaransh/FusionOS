using FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Contracts;
using FusionOS.Modules.Manufacturing.Application.WorkOrders.Contracts;
using MediatR;

namespace FusionOS.Modules.Manufacturing.Application.WorkOrders.Commands.IssueMaterialToWorkOrder;

public sealed class IssueMaterialToWorkOrderCommandHandler : IRequestHandler<IssueMaterialToWorkOrderCommand, WorkOrderDto>
{
    private readonly IWorkOrderRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public IssueMaterialToWorkOrderCommandHandler(IWorkOrderRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<WorkOrderDto> Handle(IssueMaterialToWorkOrderCommand request, CancellationToken cancellationToken)
    {
        var workOrder = await _repository.GetByIdAsync(request.CompanyId, request.WorkOrderId, cancellationToken)
            ?? throw new KeyNotFoundException($"Work order '{request.WorkOrderId}' was not found.");

        workOrder.IssueMaterial(request.ComponentProductId, request.Quantity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return WorkOrderMapper.ToDto(workOrder);
    }
}
