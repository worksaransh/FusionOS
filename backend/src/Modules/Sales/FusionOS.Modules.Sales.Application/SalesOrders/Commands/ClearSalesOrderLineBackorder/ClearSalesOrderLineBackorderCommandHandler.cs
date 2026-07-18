using FusionOS.Modules.Sales.Application.Customers.Contracts;
using FusionOS.Modules.Sales.Application.SalesOrders.Commands.CreateSalesOrder;
using FusionOS.Modules.Sales.Application.SalesOrders.Contracts;
using MediatR;

namespace FusionOS.Modules.Sales.Application.SalesOrders.Commands.ClearSalesOrderLineBackorder;

public sealed class ClearSalesOrderLineBackorderCommandHandler : IRequestHandler<ClearSalesOrderLineBackorderCommand, SalesOrderDto>
{
    private readonly ISalesOrderRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public ClearSalesOrderLineBackorderCommandHandler(ISalesOrderRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<SalesOrderDto> Handle(ClearSalesOrderLineBackorderCommand request, CancellationToken cancellationToken)
    {
        var order = await _repository.GetByIdAsync(request.CompanyId, request.SalesOrderId, cancellationToken)
            ?? throw new KeyNotFoundException($"Sales order '{request.SalesOrderId}' was not found.");

        order.ClearLineBackorder(request.LineId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateSalesOrderCommandHandler.MapToDto(order);
    }
}
