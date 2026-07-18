using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Contracts;
using FusionOS.Modules.Manufacturing.Application.WorkOrders.Contracts;
using FusionOS.Modules.Manufacturing.Domain.WorkOrders;
using MediatR;

namespace FusionOS.Modules.Manufacturing.Application.WorkOrders.Commands.CreateWorkOrder;

public sealed class CreateWorkOrderCommandHandler : IRequestHandler<CreateWorkOrderCommand, WorkOrderDto>
{
    private readonly IWorkOrderRepository _workOrderRepository;
    private readonly IBillOfMaterialsRepository _billOfMaterialsRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateWorkOrderCommandHandler(
        IWorkOrderRepository workOrderRepository,
        IBillOfMaterialsRepository billOfMaterialsRepository,
        IUnitOfWork unitOfWork)
    {
        _workOrderRepository = workOrderRepository;
        _billOfMaterialsRepository = billOfMaterialsRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<WorkOrderDto> Handle(CreateWorkOrderCommand request, CancellationToken cancellationToken)
    {
        var bom = await _billOfMaterialsRepository.GetByIdAsync(request.CompanyId, request.BillOfMaterialsId, cancellationToken)
            ?? throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.BillOfMaterialsId), $"Bill of materials '{request.BillOfMaterialsId}' does not exist for this company."),
            });

        if (!bom.IsActive)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.BillOfMaterialsId), "Cannot create a work order from a deactivated bill of materials."),
            });
        }

        // Snapshot the BOM's per-unit component quantities onto the work order so a later
        // BOM edit never retroactively changes what this order consumes.
        var snapshot = bom.Lines.Select(l => new BomComponentSnapshot(l.ComponentProductId, l.Quantity)).ToList();

        var workOrder = WorkOrder.Create(request.CompanyId, bom.Id, bom.ProductId, request.WarehouseId, request.QuantityToProduce, snapshot);

        await _workOrderRepository.AddAsync(workOrder, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return WorkOrderMapper.ToDto(workOrder);
    }
}
