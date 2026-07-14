using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Procurement.Application.PurchaseOrders.Contracts;
using FusionOS.Modules.Procurement.Application.Suppliers.Contracts;
using MediatR;

namespace FusionOS.Modules.Procurement.Application.PurchaseOrders.Commands.CreatePurchaseOrder;

public sealed class CreatePurchaseOrderCommandHandler : IRequestHandler<CreatePurchaseOrderCommand, PurchaseOrderDto>
{
    private readonly IPurchaseOrderRepository _repository;
    private readonly ISupplierRepository _supplierRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePurchaseOrderCommandHandler(IPurchaseOrderRepository repository, ISupplierRepository supplierRepository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _supplierRepository = supplierRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PurchaseOrderDto> Handle(CreatePurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        if (!await _supplierRepository.ExistsAsync(request.CompanyId, request.SupplierId, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.SupplierId), "Supplier does not exist for this company."),
            });
        }

        var order = Domain.PurchaseOrders.PurchaseOrder.Create(request.CompanyId, request.SupplierId, request.Lines);

        await _repository.AddAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(order);
    }

    internal static PurchaseOrderDto MapToDto(Domain.PurchaseOrders.PurchaseOrder order) => new(
        order.Id,
        order.SupplierId,
        order.Status.ToString(),
        order.OrderDate,
        order.TotalAmount,
        order.Lines.Select(l => new PurchaseOrderLineDto(l.Id, l.ProductId, l.Quantity, l.UnitPrice, l.LineTotal, l.ReceivedQuantity)).ToList());
}
