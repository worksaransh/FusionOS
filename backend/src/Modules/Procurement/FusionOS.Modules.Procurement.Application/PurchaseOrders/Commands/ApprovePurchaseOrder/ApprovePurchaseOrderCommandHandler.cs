using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Procurement.Application.PurchaseOrders.Commands.CreatePurchaseOrder;
using FusionOS.Modules.Procurement.Application.PurchaseOrders.Contracts;
using FusionOS.Modules.Procurement.Application.Suppliers.Contracts;
using FusionOS.SharedKernel.Context;
using MediatR;

namespace FusionOS.Modules.Procurement.Application.PurchaseOrders.Commands.ApprovePurchaseOrder;

public sealed class ApprovePurchaseOrderCommandHandler : IRequestHandler<ApprovePurchaseOrderCommand, PurchaseOrderDto>
{
    private const string CreatePermission = "procurement.purchase-order.create";
    private const string ApprovePermission = "procurement.purchase-order.approve";

    private readonly IPurchaseOrderRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserContext _currentUser;
    private readonly ISegregationOfDutiesService _sodService;

    public ApprovePurchaseOrderCommandHandler(
        IPurchaseOrderRepository repository,
        IUnitOfWork unitOfWork,
        ICurrentUserContext currentUser,
        ISegregationOfDutiesService sodService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _sodService = sodService;
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
            // Segregation-of-Duties (Phase 2.2 roadmap): if this company has an active
            // SodConflictRule explicitly naming procurement.purchase-order.create +
            // procurement.purchase-order.approve as conflicting, surface it as a SoD
            // violation (ValidationException) rather than the generic maker-checker
            // InvalidOperationException below — same underlying same-user condition,
            // but a distinct, rule-driven signal for companies that have configured it.
            if (await _sodService.HasConflictAsync(request.CompanyId, CreatePermission, ApprovePermission, cancellationToken))
            {
                throw new ValidationException(new[]
                {
                    new FluentValidation.Results.ValidationFailure(
                        nameof(request.PurchaseOrderId),
                        "Segregation of duties: the same user cannot both create and approve this purchase order."),
                });
            }

            throw new InvalidOperationException(
                "A purchase order cannot be approved by the same user who created it.");
        }

        order.Approve();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreatePurchaseOrderCommandHandler.MapToDto(order);
    }
}
