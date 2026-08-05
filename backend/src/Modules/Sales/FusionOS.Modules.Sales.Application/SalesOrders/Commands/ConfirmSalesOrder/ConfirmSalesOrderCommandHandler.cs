using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Sales.Application.Customers.Contracts;
using FusionOS.Modules.Sales.Application.SalesOrders.Commands.CreateSalesOrder;
using FusionOS.Modules.Sales.Application.SalesOrders.Contracts;
using FusionOS.SharedKernel.Context;
using MediatR;

namespace FusionOS.Modules.Sales.Application.SalesOrders.Commands.ConfirmSalesOrder;

public sealed class ConfirmSalesOrderCommandHandler : IRequestHandler<ConfirmSalesOrderCommand, SalesOrderDto>
{
    private const string CreatePermission = "sales.sales-order.create";
    private const string ConfirmPermission = "sales.sales-order.confirm";

    private readonly ISalesOrderRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserContext _currentUser;
    private readonly ISegregationOfDutiesService _sodService;

    public ConfirmSalesOrderCommandHandler(
        ISalesOrderRepository repository,
        IUnitOfWork unitOfWork,
        ICurrentUserContext currentUser,
        ISegregationOfDutiesService sodService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _sodService = sodService;
    }

    public async Task<SalesOrderDto> Handle(ConfirmSalesOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _repository.GetByIdAsync(request.CompanyId, request.SalesOrderId, cancellationToken)
            ?? throw new KeyNotFoundException($"Sales order '{request.SalesOrderId}' was not found.");

        // Segregation-of-Duties: if this company has an active SodConflictRule explicitly
        // naming sales.sales-order.create + sales.sales-order.confirm as conflicting, and the
        // confirming user is the same user who created the order, surface it as a SoD
        // violation (ValidationException). Mirrors ApprovePurchaseOrderCommandHandler.
        if (_currentUser.UserId is { } confirmerId && confirmerId == order.CreatedBy)
        {
            if (await _sodService.HasConflictAsync(request.CompanyId, CreatePermission, ConfirmPermission, cancellationToken))
            {
                throw new ValidationException(new[]
                {
                    new FluentValidation.Results.ValidationFailure(
                        nameof(request.SalesOrderId),
                        "Segregation of duties: the same user cannot both create and confirm this sales order."),
                });
            }
        }

        order.Confirm();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateSalesOrderCommandHandler.MapToDto(order);
    }
}
