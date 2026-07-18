using FusionOS.Modules.Sales.Application.Customers.Contracts;
using MediatR;

namespace FusionOS.Modules.Sales.Application.Customers.Queries.GetCustomerById;

public sealed class GetCustomerByIdQueryHandler : IRequestHandler<GetCustomerByIdQuery, CustomerDto>
{
    private readonly ICustomerRepository _repository;

    public GetCustomerByIdQueryHandler(ICustomerRepository repository) => _repository = repository;

    public async Task<CustomerDto> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var customer = await _repository.GetByIdAsync(request.CompanyId, request.CustomerId, cancellationToken)
            ?? throw new KeyNotFoundException($"Customer '{request.CustomerId}' was not found.");

        return new CustomerDto(customer.Id, customer.Name, customer.Code, customer.ContactEmail, customer.CreditLimit, customer.IsActive, customer.CreatedAt, customer.PriceListId);
    }
}
