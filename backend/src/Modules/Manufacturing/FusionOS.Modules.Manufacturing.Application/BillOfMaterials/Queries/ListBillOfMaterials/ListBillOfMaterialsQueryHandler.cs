using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Contracts;
using MediatR;

namespace FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Queries.ListBillOfMaterials;

public sealed class ListBillOfMaterialsQueryHandler : IRequestHandler<ListBillOfMaterialsQuery, PagedResult<BillOfMaterialsDto>>
{
    private readonly IBillOfMaterialsRepository _repository;

    public ListBillOfMaterialsQueryHandler(IBillOfMaterialsRepository repository) => _repository = repository;

    public async Task<PagedResult<BillOfMaterialsDto>> Handle(ListBillOfMaterialsQuery request, CancellationToken cancellationToken)
    {
        var boms = await _repository.ListAsync(request.CompanyId, request.Search, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, request.Search, cancellationToken);

        var dtos = boms.Select(BillOfMaterialsMapper.ToDto).ToList();

        return new PagedResult<BillOfMaterialsDto>(dtos, request.Page, request.PageSize, total);
    }
}
