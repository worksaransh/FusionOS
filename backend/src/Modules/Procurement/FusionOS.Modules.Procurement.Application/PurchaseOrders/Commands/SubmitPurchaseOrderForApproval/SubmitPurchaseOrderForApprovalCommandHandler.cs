using FusionOS.Modules.Procurement.Application.PurchaseOrders.Commands.CreatePurchaseOrder;
using FusionOS.Modules.Procurement.Application.PurchaseOrders.Contracts;
using FusionOS.Modules.Procurement.Application.Suppliers.Contracts;
using FusionOS.SharedKernel.Context;
using MediatR;

namespace FusionOS.Modules.Procurement.Application.PurchaseOrders.Commands.SubmitPurchaseOrderForApproval;

public sealed class SubmitPurchaseOrderForApprovalCommandHandler : IRequestHandler<SubmitPurchaseOrderForApprovalCommand, PurchaseOrderDto>
{
    private readonly IPurchaseOrderRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserContext _currentUser;

    public SubmitPurchaseOrderForApprovalCommandHandler(IPurchaseOrderRepository repository, IUnitOfWork unitOfWork, ICurrentUserContext currentUser)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<PurchaseOrderDto> Handle(SubmitPurchaseOrderForApprovalCommand request, CancellationToken cancellationToken)
    {
        var order = await _repository.GetByIdAsync(request.CompanyId, request.PurchaseOrderId, cancellationToken)
            ?? throw new KeyNotFoundException($"Purchase order '{request.PurchaseOrderId}' was not found.");

        var requestedBy = _currentUser.UserId
            ?? throw new InvalidOperationException("A user context is required to submit a purchase order for approval.");

        order.SubmitForApproval(requestedBy);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreatePurchaseOrderCommandHandler.MapToDto(order);
    }
}
