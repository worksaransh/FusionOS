using FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Contracts;
using MediatR;

namespace FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Queries.GetBillOfMaterialsById;

public sealed class GetBillOfMaterialsByIdQueryHandler : IRequestHandler<GetBillOfMaterialsByIdQuery, BillOfMaterialsDto>
{
    private readonly IBillOfMaterialsRepository _repository;

    public GetBillOfMaterialsByIdQueryHandler(IBillOfMaterialsRepository repository) => _repository = repository;

    public async Task<BillOfMaterialsDto> Handle(GetBillOfMaterialsByIdQuery request, CancellationToken cancellationToken)
    {
        var bom = await _repository.GetByIdAsync(request.CompanyId, request.BillOfMaterialsId, cancellationToken)
            ?? throw new KeyNotFoundException($"Bill of materials '{request.BillOfMaterialsId}' was not found.");

        return BillOfMaterialsMapper.ToDto(bom);
    }
}
