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

        // Cross-aggregate quantity guard (2026-07-14 coverage-audit follow-up,
        // tightened 2026-07-20): the cumulative dispatched quantity per product -
        // every existing dispatch against this sales order plus every line of this
        // request - must never exceed the quantity the order actually ordered.
        //
        // Counting rule: ALL persisted dispatches count toward the cap - a
        // Dispatch has no status/lifecycle at all (no cancelled or returned
        // state), so every persisted dispatch line is goods that physically left
        // against this order. If a cancellation/return state is ever introduced,
        // IDispatchRepository.GetDispatchedQuantityAsync must be updated to
        // exclude it - the decision lives in that query, not here.
        //
        // Request lines are grouped by product before checking, so the same
        // product split across several request lines cannot slip past the cap by
        // each line passing individually; the cap itself sums every order line
        // carrying the product, in case the order lists a product more than once.
        var failures = new List<FluentValidation.Results.ValidationFailure>();
        foreach (var productLines in request.Lines.GroupBy(l => l.ProductId))
        {
            if (!salesOrder.Lines.Any(l => l.ProductId == productLines.Key))
            {
                failures.Add(new FluentValidation.Results.ValidationFailure(
                    nameof(request.Lines), $"Product {productLines.Key} is not part of sales order {request.SalesOrderId}."));
                continue;
            }

            var orderedQuantity = salesOrder.Lines.Where(l => l.ProductId == productLines.Key).Sum(l => l.Quantity);
            var requestedQuantity = productLines.Sum(l => l.QuantityDispatched);
            var alreadyDispatched = await _repository.GetDispatchedQuantityAsync(request.CompanyId, request.SalesOrderId, productLines.Key, cancellationToken);
            if (alreadyDispatched + requestedQuantity > orderedQuantity)
            {
                failures.Add(new FluentValidation.Results.ValidationFailure(
                    nameof(request.Lines),
                    $"Product {productLines.Key}: dispatching {requestedQuantity} would exceed the sales order's remaining dispatchable quantity " +
                    $"({orderedQuantity - alreadyDispatched} of {orderedQuantity} left, {alreadyDispatched} already dispatched)."));
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
