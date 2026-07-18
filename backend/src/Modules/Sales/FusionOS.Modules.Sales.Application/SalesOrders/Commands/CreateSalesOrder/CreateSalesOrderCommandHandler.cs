using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Sales.Application.Customers.Contracts;
using FusionOS.Modules.Sales.Application.SalesOrders.Contracts;
using MediatR;

namespace FusionOS.Modules.Sales.Application.SalesOrders.Commands.CreateSalesOrder;

public sealed class CreateSalesOrderCommandHandler : IRequestHandler<CreateSalesOrderCommand, SalesOrderDto>
{
    /// <summary>
    /// A line discount above this hardcoded percentage is rejected outright rather
    /// than routed through the generic Workflow/Approval engine (Phase M7) for a
    /// human sign-off — a full resubmit-for-approval flow is a larger, separate
    /// piece of scope with no spec behind it yet. Documented placeholder, not a
    /// per-company configurable setting, same restraint as the Dashboard's
    /// hardcoded 10-unit low-stock threshold (Phase M6) and Putaway's
    /// first-active-bin heuristic (Phase M9).
    /// </summary>
    private const decimal MaxDiscountPercentageWithoutApproval = 20m;

    private readonly ISalesOrderRepository _repository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateSalesOrderCommandHandler(ISalesOrderRepository repository, ICustomerRepository customerRepository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<SalesOrderDto> Handle(CreateSalesOrderCommand request, CancellationToken cancellationToken)
    {
        if (!await _customerRepository.ExistsAsync(request.CompanyId, request.CustomerId, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.CustomerId), "Customer does not exist for this company."),
            });
        }

        foreach (var line in request.Lines)
        {
            if (line.DiscountPercentage > MaxDiscountPercentageWithoutApproval)
            {
                throw new ValidationException(new[]
                {
                    new FluentValidation.Results.ValidationFailure(
                        nameof(request.Lines),
                        $"A line discount over {MaxDiscountPercentageWithoutApproval}% requires approval, which this slice does not yet support — lower the discount or split the order."),
                });
            }
        }

        var order = Domain.SalesOrders.SalesOrder.Create(request.CompanyId, request.CustomerId, request.Lines);

        await _repository.AddAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(order);
    }

    internal static SalesOrderDto MapToDto(Domain.SalesOrders.SalesOrder order) => new(
        order.Id,
        order.CustomerId,
        order.Status.ToString(),
        order.OrderDate,
        order.TotalAmount,
        order.Lines.Select(l => new SalesOrderLineDto(l.Id, l.ProductId, l.Quantity, l.UnitPrice, l.DiscountPercentage, l.LineTotal)).ToList());
}
