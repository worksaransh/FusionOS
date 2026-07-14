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
