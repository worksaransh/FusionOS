using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Sales.Application.Returns.Commands.CreateSalesReturn;
using FusionOS.Modules.Sales.Application.Returns.Contracts;
using MediatR;

namespace FusionOS.Modules.Sales.Application.Returns.Queries.ListSalesReturns;

public sealed class ListSalesReturnsQueryHandler : IRequestHandler<ListSalesReturnsQuery, PagedResult<SalesReturnDto>>
{
    private readonly ISalesReturnRepository _repository;

    public ListSalesReturnsQueryHandler(ISalesReturnRepository repository) => _repository = repository;

    public async Task<PagedResult<SalesReturnDto>> Handle(ListSalesReturnsQuery request, CancellationToken cancellationToken)
    {
        var salesReturns = await _repository.ListAsync(request.CompanyId, request.Page, request.PageSize, request.Status, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, request.Status, cancellationToken);

        var dtos = salesReturns.Select(CreateSalesReturnCommandHandler.MapToDto).ToList();

        return new PagedResult<SalesReturnDto>(dtos, request.Page, request.PageSize, total);
    }
}
