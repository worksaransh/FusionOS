using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Sales.Application.Customers.Contracts;
using FusionOS.Modules.Sales.Application.Dispatches.Contracts;
using FusionOS.Modules.Sales.Application.SalesOrders.Contracts;
using MediatR;

namespace FusionOS.Modules.Sales.Application.Dispatches.Commands.CreateDispatch;

public sealed class CreateDispatchCommandHandler : IRequestHandler<CreateDispatchCommand, DispatchDto>
{
    private readonly IDispatchRepository _repository;
    private readonly ISalesOrderRepository _salesOrderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateDispatchCommandHandler(IDispatchRepository repository, ISalesOrderRepository salesOrderRepository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _salesOrderRepository = salesOrderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<DispatchDto> Handle(CreateDispatchCommand request, CancellationToken cancellationToken)
    {
        var salesOrder = await _salesOrderRepository.GetByIdAsync(request.CompanyId, request.SalesOrderId, cancellationToken);
        if (salesOrder is null)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.SalesOrderId), "Sales order does not exist for this company."),
            });
        }

        // 2026-07-14 coverage-audit follow-up: previously nothing compared the
        // requested dispatch lines to what the sales order actually ordered, or to
        // what had already been dispatched against it - an order could be dispatched
        // for any quantity, any number of times. Reject any line that would push
        // the cumulative dispatched quantity for that product past what was ordered.
        var failures = new List<FluentValidation.Results.ValidationFailure>();
        foreach (var line in request.Lines)
        {
            var orderLine = salesOrder.Lines.FirstOrDefault(l => l.ProductId == line.ProductId);
            if (orderLine is null)
            {
                failures.Add(new FluentValidation.Results.ValidationFailure(
                    nameof(request.Lines), $"Product {line.ProductId} is not part of sales order {request.SalesOrderId}."));
                continue;
            }

            var alreadyDispatched = await _repository.GetDispatchedQuantityAsync(request.CompanyId, request.SalesOrderId, line.ProductId, cancellationToken);
            if (alreadyDispatched + line.QuantityDispatched > orderLine.Quantity)
            {
                failures.Add(new FluentValidation.Results.ValidationFailure(
                    nameof(request.Lines),
                    $"Product {line.ProductId}: dispatching {line.QuantityDispatched} would exceed the sales order's remaining dispatchable quantity " +
                    $"({orderLine.Quantity - alreadyDispatched} of {orderLine.Quantity} left, {alreadyDispatched} already dispatched)."));
            }
        }

        if (failures.Count > 0)
            throw new ValidationException(failures);

        var dispatch = Domain.Dispatches.Dispatch.Create(request.CompanyId, request.SalesOrderId, request.WarehouseId, request.Lines);

        await _repository.AddAsync(dispatch, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(dispatch);
    }

    internal static DispatchDto MapToDto(Domain.Dispatches.Dispatch dispatch) => new(
        dispatch.Id,
        dispatch.SalesOrderId,
        dispatch.WarehouseId,
        dispatch.DispatchDate,
        dispatch.Lines.Select(l => new DispatchLineDto(l.Id, l.ProductId, l.QuantityDispatched)).ToList());
}
