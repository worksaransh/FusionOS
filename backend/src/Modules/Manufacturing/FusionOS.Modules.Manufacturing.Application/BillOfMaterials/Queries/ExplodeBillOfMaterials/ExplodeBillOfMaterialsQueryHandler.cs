using FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Contracts;
using MediatR;

namespace FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Queries.ExplodeBillOfMaterials;

public sealed class ExplodeBillOfMaterialsQueryHandler : IRequestHandler<ExplodeBillOfMaterialsQuery, BomExplosionResultDto>
{
    private readonly IBillOfMaterialsRepository _repository;

    public ExplodeBillOfMaterialsQueryHandler(IBillOfMaterialsRepository repository) => _repository = repository;

    public async Task<BomExplosionResultDto> Handle(ExplodeBillOfMaterialsQuery request, CancellationToken cancellationToken)
    {
        var bom = await _repository.GetByIdAsync(request.CompanyId, request.BillOfMaterialsId, cancellationToken)
            ?? throw new KeyNotFoundException($"Bill of materials '{request.BillOfMaterialsId}' was not found.");

        var (leafTotals, warnings) = await BomExplosionEngine.ExplodeAsync(_repository, request.CompanyId, bom, request.Quantity, cancellationToken);

        // Descending by quantity — see BomExplosionResultDto's doc comment for why.
        var leafRequirements = leafTotals
            .Select(kvp => new BomExplosionLineDto(kvp.Key, kvp.Value))
            .OrderByDescending(l => l.TotalQuantityRequired)
            .ToList();

        return new BomExplosionResultDto(bom.Id, request.Quantity, leafRequirements, warnings);
    }
}
