using FluentValidation;

namespace FusionOS.Modules.Sales.Application.Discounts.Commands.UpdateDiscountRule;

public sealed class UpdateDiscountRuleValidator : AbstractValidator<UpdateDiscountRuleCommand>
{
    public UpdateDiscountRuleValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.DiscountRuleId).NotEmpty();
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.MinQuantity).GreaterThan(0);
        RuleFor(x => x.DiscountPercentage).GreaterThan(0).LessThanOrEqualTo(100);
    }
}
