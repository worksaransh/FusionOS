using FusionOS.Modules.Procurement.Application.PurchaseOrders.Commands.CreatePurchaseOrder;
using FusionOS.Modules.Procurement.Application.PurchaseOrders.Contracts;
using FusionOS.Modules.Procurement.Application.Suppliers.Contracts;
using FusionOS.SharedKernel.Context;
using MediatR;

namespace FusionOS.Modules.Procurement.Application.PurchaseOrders.Commands.ApprovePurchaseOrder;

public sealed class ApprovePurchaseOrderCommandHandler : IRequestHandler<ApprovePurchaseOrderCommand, PurchaseOrderDto>
{
    private readonly IPurchaseOrderRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserContext _currentUser;

    public ApprovePurchaseOrderCommandHandler(IPurchaseOrderRepository repository, IUnitOfWork unitOfWork, ICurrentUserContext currentUser)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<PurchaseOrderDto> Handle(ApprovePurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _repository.GetByIdAsync(request.CompanyId, request.PurchaseOrderId, cancellationToken)
            ?? throw new KeyNotFoundException($"Purchase order '{request.PurchaseOrderId}' was not found.");

        // Maker-checker fix (2026-07-14 sprint audit gap): a Purchase Order must not
        // be approved by the same user who created it. CreatedBy is stamped
        // automatically at Create-time by BaseDbContext.StampAudit() off
        // ICurrentUserContext.UserId (see TenantAggregateRoot), so it is already a
        // reliable record of the requesting/creating user with no domain model
        // change needed here.
        if (_currentUser.UserId is { } approverId && approverId == order.CreatedBy)
        {
            throw new InvalidOperationException(
                "A purchase order cannot be approved by the same user who created it.");
        }

        order.Approve();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreatePurchaseOrderCommandHandler.MapToDto(order);
    }
}
