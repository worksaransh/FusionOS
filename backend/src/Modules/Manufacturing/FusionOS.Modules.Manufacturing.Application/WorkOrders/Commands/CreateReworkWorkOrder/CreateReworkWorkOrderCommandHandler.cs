using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Contracts;
using FusionOS.Modules.Manufacturing.Application.WorkOrders.Contracts;
using FusionOS.Modules.Manufacturing.Domain.WorkOrders;
using MediatR;

namespace FusionOS.Modules.Manufacturing.Application.WorkOrders.Commands.CreateReworkWorkOrder;

public sealed class CreateReworkWorkOrderCommandHandler : IRequestHandler<CreateReworkWorkOrderCommand, WorkOrderDto>
{
    private readonly IWorkOrderRepository _workOrderRepository;
    private readonly IBillOfMaterialsRepository _billOfMaterialsRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateReworkWorkOrderCommandHandler(
        IWorkOrderRepository workOrderRepository,
        IBillOfMaterialsRepository billOfMaterialsRepository,
        IUnitOfWork unitOfWork)
    {
        _workOrderRepository = workOrderRepository;
        _billOfMaterialsRepository = billOfMaterialsRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<WorkOrderDto> Handle(CreateReworkWorkOrderCommand request, CancellationToken cancellationToken)
    {
        var sourceWorkOrder = await _workOrderRepository.GetByIdAsync(request.CompanyId, request.SourceWorkOrderId, cancellationToken)
            ?? throw new KeyNotFoundException($"Work order '{request.SourceWorkOrderId}' was not found.");

        // Throws if the source order isn't Completed, or if the quantity exceeds its
        // remaining unclaimed scrap — let that domain exception propagate, same as
        // every other handler that calls straight into the aggregate.
        sourceWorkOrder.RecordRework(request.QuantityToRework);

        var bom = await _billOfMaterialsRepository.GetByIdAsync(request.CompanyId, sourceWorkOrder.BillOfMaterialsId, cancellationToken)
            ?? throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(sourceWorkOrder.BillOfMaterialsId), $"Bill of materials '{sourceWorkOrder.BillOfMaterialsId}' does not exist for this company."),
            });

        // Snapshot the BOM's components and operations exactly like CreateWorkOrderCommandHandler.
        var componentSnapshot = bom.Lines.Select(l => new BomComponentSnapshot(l.ComponentProductId, l.Quantity, l.UnitCost)).ToList();
        var operationSnapshot = bom.Operations
            .Select(o => new RoutingOperationSnapshot(o.Id, o.SequenceNumber, o.OperationName, o.WorkCenter, o.StandardMinutes))
            .ToList();

        var reworkWorkOrder = WorkOrder.Create(
            request.CompanyId,
            bom.Id,
            bom.ProductId,
            sourceWorkOrder.WarehouseId,
            request.QuantityToRework,
            componentSnapshot,
            operationSnapshot,
            sourceWorkOrderId: sourceWorkOrder.Id);

        await _workOrderRepository.AddAsync(reworkWorkOrder, cancellationToken);

        // Single SaveChanges — EF change tracking makes the source order's QuantityReworked
        // increment and the new work order's insert atomic in one transaction.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return WorkOrderMapper.ToDto(reworkWorkOrder);
    }
}
