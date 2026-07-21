using FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Contracts;
using FusionOS.Modules.Manufacturing.Application.WorkOrders.Contracts;
using MediatR;

namespace FusionOS.Modules.Manufacturing.Application.WorkOrders.Commands.ReturnMaterialFromWorkOrder;

public sealed class ReturnMaterialFromWorkOrderCommandHandler : IRequestHandler<ReturnMaterialFromWorkOrderCommand, WorkOrderDto>
{
    private readonly IWorkOrderRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public ReturnMaterialFromWorkOrderCommandHandler(IWorkOrderRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<WorkOrderDto> Handle(ReturnMaterialFromWorkOrderCommand request, CancellationToken cancellationToken)
    {
        var workOrder = await _repository.GetByIdAsync(request.CompanyId, request.WorkOrderId, cancellationToken)
            ?? throw new KeyNotFoundException($"Work order '{request.WorkOrderId}' was not found.");

        workOrder.ReturnMaterial(request.ComponentProductId, request.Quantity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return WorkOrderMapper.ToDto(workOrder);
    }
}
