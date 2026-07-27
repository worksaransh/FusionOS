using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Sales.Application.Dispatches.Commands.CreateDispatch;
using FusionOS.Modules.Sales.Application.Dispatches.Contracts;
using MediatR;

namespace FusionOS.Modules.Sales.Application.Dispatches.Queries.ListMyDispatches;

public sealed class ListMyDispatchesQueryHandler : IRequestHandler<ListMyDispatchesQuery, PagedResult<DispatchDto>>
{
    private readonly IDispatchRepository _repository;

    public ListMyDispatchesQueryHandler(IDispatchRepository repository) => _repository = repository;

    public async Task<PagedResult<DispatchDto>> Handle(ListMyDispatchesQuery request, CancellationToken cancellationToken)
    {
        var dispatches = await _repository.ListByCustomerAsync(request.CustomerId, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountByCustomerAsync(request.CustomerId, cancellationToken);

        var dtos = dispatches.Select(CreateDispatchCommandHandler.MapToDto).ToList();

        return new PagedResult<DispatchDto>(dtos, request.Page, request.PageSize, total);
    }
}
