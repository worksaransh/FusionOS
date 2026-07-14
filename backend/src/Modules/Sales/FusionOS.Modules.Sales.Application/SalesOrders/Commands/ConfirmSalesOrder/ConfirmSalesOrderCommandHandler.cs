using FusionOS.Modules.Sales.Application.Customers.Contracts;
using FusionOS.Modules.Sales.Application.SalesOrders.Commands.CreateSalesOrder;
using FusionOS.Modules.Sales.Application.SalesOrders.Contracts;
using MediatR;

namespace FusionOS.Modules.Sales.Application.SalesOrders.Commands.ConfirmSalesOrder;

public sealed class ConfirmSalesOrderCommandHandler : IRequestHandler<ConfirmSalesOrderCommand, SalesOrderDto>
{
    private readonly ISalesOrderRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public ConfirmSalesOrderCommandHandler(ISalesOrderRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<SalesOrderDto> Handle(ConfirmSalesOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _repository.GetByIdAsync(request.CompanyId, request.SalesOrderId, cancellationToken)
            ?? throw new KeyNotFoundException($"Sales order '{request.SalesOrderId}' was not found.");

        order.Confirm();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateSalesOrderCommandHandler.MapToDto(order);
    }
}
