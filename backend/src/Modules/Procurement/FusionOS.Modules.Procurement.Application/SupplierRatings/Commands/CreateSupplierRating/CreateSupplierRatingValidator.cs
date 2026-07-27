using FluentValidation;
using FusionOS.Modules.Procurement.Domain.SupplierRatings;

namespace FusionOS.Modules.Procurement.Application.SupplierRatings.Commands.CreateSupplierRating;

public sealed class CreateSupplierRatingValidator : AbstractValidator<CreateSupplierRatingCommand>
{
    public CreateSupplierRatingValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.SupplierId).NotEmpty();
        RuleFor(x => x.RatedByUserId).NotEmpty();
        RuleFor(x => x.QualityScore).InclusiveBetween(SupplierRating.MinScore, SupplierRating.MaxScore);
        RuleFor(x => x.DeliveryScore).InclusiveBetween(SupplierRating.MinScore, SupplierRating.MaxScore);
        RuleFor(x => x.Comments).MaximumLength(2000);
    }
}
