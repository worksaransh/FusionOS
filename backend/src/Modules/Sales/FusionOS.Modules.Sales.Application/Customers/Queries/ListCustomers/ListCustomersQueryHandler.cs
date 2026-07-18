using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Sales.Application.Customers.Contracts;
using MediatR;

namespace FusionOS.Modules.Sales.Application.Customers.Queries.ListCustomers;

public sealed class ListCustomersQueryHandler : IRequestHandler<ListCustomersQuery, PagedResult<CustomerDto>>
{
    private readonly ICustomerRepository _repository;

    public ListCustomersQueryHandler(ICustomerRepository repository) => _repository = repository;

    public async Task<PagedResult<CustomerDto>> Handle(ListCustomersQuery request, CancellationToken cancellationToken)
    {
        var customers = await _repository.ListAsync(request.CompanyId, request.Search, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, request.Search, cancellationToken);

        var dtos = customers
            .Select(c => new CustomerDto(c.Id, c.Name, c.Code, c.ContactEmail, c.CreditLimit, c.IsActive, c.CreatedAt, c.PriceListId))
            .ToList();

        return new PagedResult<CustomerDto>(dtos, request.Page, request.PageSize, total);
    }
}
