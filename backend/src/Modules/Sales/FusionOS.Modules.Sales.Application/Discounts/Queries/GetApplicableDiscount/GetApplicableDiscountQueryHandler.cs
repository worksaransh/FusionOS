using FusionOS.Modules.Sales.Application.Discounts.Contracts;
using MediatR;

namespace FusionOS.Modules.Sales.Application.Discounts.Queries.GetApplicableDiscount;

/// <summary>
/// The "tiered" part of the tiered discount rules engine: among every active
/// DiscountRule for this Product whose MinQuantity is met by the given
/// Quantity, picks the one with the highest MinQuantity (the deepest tier the
/// order actually reaches) rather than the highest discount percentage — a
/// well-formed tier schedule has both move together, but MinQuantity is the
/// unambiguous ordering key if a catalog is ever set up with overlapping or
/// out-of-order percentages.
/// </summary>
public sealed class GetApplicableDiscountQueryHandler : IRequestHandler<GetApplicableDiscountQuery, ApplicableDiscountDto>
{
    private readonly IDiscountRuleRepository _repository;

    public GetApplicableDiscountQueryHandler(IDiscountRuleRepository repository) => _repository = repository;

    public async Task<ApplicableDiscountDto> Handle(GetApplicableDiscountQuery request, CancellationToken cancellationToken)
    {
        var rules = await _repository.ListActiveForProductAsync(request.CompanyId, request.ProductId, cancellationToken);

        var bestTier = rules
            .Where(r => request.Quantity >= r.MinQuantity)
            .OrderByDescending(r => r.MinQuantity)
            .FirstOrDefault();

        return bestTier is null
            ? new ApplicableDiscountDto(null, 0m)
            : new ApplicableDiscountDto(bestTier.Id, bestTier.DiscountPercentage);
    }
}
