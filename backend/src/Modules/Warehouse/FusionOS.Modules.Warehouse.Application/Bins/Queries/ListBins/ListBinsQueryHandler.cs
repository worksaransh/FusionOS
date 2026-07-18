using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Warehouse.Application.Bins.Contracts;
using MediatR;

namespace FusionOS.Modules.Warehouse.Application.Bins.Queries.ListBins;

public sealed class ListBinsQueryHandler : IRequestHandler<ListBinsQuery, PagedResult<BinDto>>
{
    private readonly IBinRepository _repository;

    public ListBinsQueryHandler(IBinRepository repository) => _repository = repository;

    public async Task<PagedResult<BinDto>> Handle(ListBinsQuery request, CancellationToken cancellationToken)
    {
        var bins = await _repository.ListAsync(request.CompanyId, request.ZoneId, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, request.ZoneId, cancellationToken);

        var dtos = bins.Select(b => new BinDto(b.Id, b.ZoneId, b.Name, b.Code, b.IsActive, b.CreatedAt)).ToList();

        return new PagedResult<BinDto>(dtos, request.Page, request.PageSize, total);
    }
}
