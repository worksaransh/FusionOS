using FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Contracts;
using MediatR;

namespace FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Queries.GetVersionHistory;

public sealed class GetVersionHistoryQueryHandler : IRequestHandler<GetVersionHistoryQuery, IReadOnlyList<BillOfMaterialsDto>>
{
    private readonly IBillOfMaterialsRepository _repository;

    public GetVersionHistoryQueryHandler(IBillOfMaterialsRepository repository) => _repository = repository;

    public async Task<IReadOnlyList<BillOfMaterialsDto>> Handle(GetVersionHistoryQuery request, CancellationToken cancellationToken)
    {
        var bom = await _repository.GetByIdAsync(request.CompanyId, request.BillOfMaterialsId, cancellationToken)
            ?? throw new KeyNotFoundException($"Bill of materials '{request.BillOfMaterialsId}' was not found.");

        var history = await _repository.ListVersionHistoryAsync(request.CompanyId, bom.Id, cancellationToken);
        return history.Select(BillOfMaterialsMapper.ToDto).ToList();
    }
}
