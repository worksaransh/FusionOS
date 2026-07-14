using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Sales.Application.Dispatches.Commands.CreateDispatch;
using FusionOS.Modules.Sales.Application.Dispatches.Contracts;
using MediatR;

namespace FusionOS.Modules.Sales.Application.Dispatches.Queries.ListDispatches;

public sealed class ListDispatchesQueryHandler : IRequestHandler<ListDispatchesQuery, PagedResult<DispatchDto>>
{
    private readonly IDispatchRepository _repository;

    public ListDispatchesQueryHandler(IDispatchRepository repository) => _repository = repository;

    public async Task<PagedResult<DispatchDto>> Handle(ListDispatchesQuery request, CancellationToken cancellationToken)
    {
        var dispatches = await _repository.ListAsync(request.CompanyId, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, cancellationToken);

        var dtos = dispatches.Select(CreateDispatchCommandHandler.MapToDto).ToList();

        return new PagedResult<DispatchDto>(dtos, request.Page, request.PageSize, total);
    }
}
