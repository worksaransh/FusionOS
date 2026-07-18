using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Procurement.Application.PurchaseOrders.Contracts;
using FusionOS.Modules.Procurement.Application.Suppliers.Contracts;
using FusionOS.Modules.Procurement.Application.VendorReturns.Contracts;
using MediatR;

namespace FusionOS.Modules.Procurement.Application.VendorReturns.Commands.CreateVendorReturn;

/// <summary>
/// Guards against returning more than was actually received: PurchaseOrderLine
/// itself deliberately never rejects over-receipt (see its own doc comment),
/// and there is no "quantity returned" concept anywhere on PurchaseOrder — so
/// this handler computes it fresh each time from every prior non-Cancelled
/// VendorReturn against the same PurchaseOrder/Product pair.
/// </summary>
public sealed class CreateVendorReturnCommandHandler : IRequestHandler<CreateVendorReturnCommand, VendorReturnDto>
{
    private readonly IVendorReturnRepository _repository;
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateVendorReturnCommandHandler(IVendorReturnRepository repository, IPurchaseOrderRepository purchaseOrderRepository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _purchaseOrderRepository = purchaseOrderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<VendorReturnDto> Handle(CreateVendorReturnCommand request, CancellationToken cancellationToken)
    {
        var purchaseOrder = await _purchaseOrderRepository.GetByIdAsync(request.CompanyId, request.PurchaseOrderId, cancellationToken)
            ?? throw new KeyNotFoundException($"Purchase order '{request.PurchaseOrderId}' was not found.");

        var line = purchaseOrder.Lines.FirstOrDefault(l => l.ProductId == request.ProductId);
        if (line is null)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.ProductId), "This product was not ordered on the given purchase order."),
            });
        }

        var alreadyReturned = await _repository.SumReturnedQuantityAsync(request.CompanyId, request.PurchaseOrderId, request.ProductId, cancellationToken);
        var returnable = line.ReceivedQuantity - alreadyReturned;
        if (request.Quantity > returnable)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.Quantity),
                    $"Cannot return more than was received and not already returned: {returnable} available, {request.Quantity} requested."),
            });
        }

        var vendorReturn = Domain.VendorReturns.VendorReturn.Create(
            request.CompanyId, request.PurchaseOrderId, request.ProductId, request.WarehouseId, request.Quantity, request.Reason);

        await _repository.AddAsync(vendorReturn, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return VendorReturnMapper.ToDto(vendorReturn);
    }
}
