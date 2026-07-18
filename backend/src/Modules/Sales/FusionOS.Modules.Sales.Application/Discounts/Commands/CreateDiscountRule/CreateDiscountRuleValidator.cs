using FluentValidation;

namespace FusionOS.Modules.Sales.Application.Discounts.Commands.CreateDiscountRule;

public sealed class CreateDiscountRuleValidator : AbstractValidator<CreateDiscountRuleCommand>
{
    public CreateDiscountRuleValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.MinQuantity).GreaterThan(0);
        RuleFor(x => x.DiscountPercentage).GreaterThan(0).LessThanOrEqualTo(100);
    }
}
