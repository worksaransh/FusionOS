using FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Contracts;
using MediatR;

namespace FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Queries.GenerateMrpRequirements;

public sealed class GenerateMrpRequirementsQueryHandler : IRequestHandler<GenerateMrpRequirementsQuery, MrpRequirementsResultDto>
{
    private readonly IBillOfMaterialsRepository _repository;

    public GenerateMrpRequirementsQueryHandler(IBillOfMaterialsRepository repository) => _repository = repository;

    public async Task<MrpRequirementsResultDto> Handle(GenerateMrpRequirementsQuery request, CancellationToken cancellationToken)
    {
        var totals = new Dictionary<Guid, decimal>();
        var warnings = new List<string>();

        foreach (var demandLine in request.DemandLines)
        {
            var bom = await _repository.GetPrimaryByProductIdAsync(request.CompanyId, demandLine.ProductId, cancellationToken);
            if (bom is null)
            {
                warnings.Add($"Product '{demandLine.ProductId}' has no active bill of materials — cannot plan requirements for it.");
                continue;
            }

            // Same recursive explosion engine ExplodeBillOfMaterialsQuery uses — every demand
            // line's leaf requirements are merged into ONE company-wide running total below,
            // rather than kept per-demand-line, since MRP's job is the aggregate "how much of
            // everything do all these demands together require".
            var (leafTotals, lineWarnings) = await BomExplosionEngine.ExplodeAsync(_repository, request.CompanyId, bom, demandLine.Quantity, cancellationToken);

            warnings.AddRange(lineWarnings);
            foreach (var (productId, quantity) in leafTotals)
                totals[productId] = totals.TryGetValue(productId, out var existing) ? existing + quantity : quantity;
        }

        var requirements = totals
            .Select(kvp => new MrpRequirementLineDto(kvp.Key, kvp.Value))
            .OrderByDescending(r => r.TotalQuantityRequired)
            .ToList();

        return new MrpRequirementsResultDto(requirements, warnings);
    }
}
