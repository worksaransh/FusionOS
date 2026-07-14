using FusionOS.Modules.Procurement.Application.PurchaseOrders.Commands.CreatePurchaseOrder;
using FusionOS.Modules.Procurement.Application.PurchaseOrders.Contracts;
using FusionOS.Modules.Procurement.Application.Suppliers.Contracts;
using MediatR;

namespace FusionOS.Modules.Procurement.Application.PurchaseOrders.Commands.ApprovePurchaseOrder;

public sealed class ApprovePurchaseOrderCommandHandler : IRequestHandler<ApprovePurchaseOrderCommand, PurchaseOrderDto>
{
    private readonly IPurchaseOrderRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public ApprovePurchaseOrderCommandHandler(IPurchaseOrderRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PurchaseOrderDto> Handle(ApprovePurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _repository.GetByIdAsync(request.CompanyId, request.PurchaseOrderId, cancellationToken)
            ?? throw new KeyNotFoundException($"Purchase order '{request.PurchaseOrderId}' was not found.");

        order.Approve();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreatePurchaseOrderCommandHandler.MapToDto(order);
    }
}
