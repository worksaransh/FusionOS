using FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Contracts;
using MediatR;

namespace FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Queries.ListBillOfMaterialsByProduct;

public sealed class ListBillOfMaterialsByProductQueryHandler : IRequestHandler<ListBillOfMaterialsByProductQuery, IReadOnlyList<BillOfMaterialsDto>>
{
    private readonly IBillOfMaterialsRepository _repository;

    public ListBillOfMaterialsByProductQueryHandler(IBillOfMaterialsRepository repository) => _repository = repository;

    public async Task<IReadOnlyList<BillOfMaterialsDto>> Handle(ListBillOfMaterialsByProductQuery request, CancellationToken cancellationToken)
    {
        var boms = await _repository.ListActiveByProductIdAsync(request.CompanyId, request.ProductId, cancellationToken);
        return boms.Select(BillOfMaterialsMapper.ToDto).ToList();
    }
}
