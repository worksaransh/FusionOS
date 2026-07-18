using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Sales.Application.Customers.Contracts;
using FusionOS.Modules.Sales.Application.PriceLists.Contracts;
using MediatR;

namespace FusionOS.Modules.Sales.Application.Customers.Commands.AssignPriceList;

public sealed class AssignPriceListCommandHandler : IRequestHandler<AssignPriceListCommand, CustomerDto>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IPriceListRepository _priceListRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AssignPriceListCommandHandler(ICustomerRepository customerRepository, IPriceListRepository priceListRepository, IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _priceListRepository = priceListRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CustomerDto> Handle(AssignPriceListCommand request, CancellationToken cancellationToken)
    {
        var customer = await _customerRepository.GetByIdAsync(request.CompanyId, request.CustomerId, cancellationToken)
            ?? throw new KeyNotFoundException($"Customer '{request.CustomerId}' was not found.");

        if (request.PriceListId is { } priceListId &&
            !await _priceListRepository.ExistsAsync(request.CompanyId, priceListId, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.PriceListId), "Price list does not exist for this company."),
            });
        }

        customer.AssignPriceList(request.PriceListId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CustomerDto(customer.Id, customer.Name, customer.Code, customer.ContactEmail, customer.CreditLimit, customer.IsActive, customer.CreatedAt, customer.PriceListId);
    }
}
