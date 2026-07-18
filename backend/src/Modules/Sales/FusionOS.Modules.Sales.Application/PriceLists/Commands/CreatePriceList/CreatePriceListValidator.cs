using FluentValidation;

namespace FusionOS.Modules.Sales.Application.PriceLists.Commands.CreatePriceList;

public sealed class CreatePriceListValidator : AbstractValidator<CreatePriceListCommand>
{
    public CreatePriceListValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Entries).NotEmpty().WithMessage("A price list must have at least one entry.");
        RuleForEach(x => x.Entries).ChildRules(entry =>
        {
            entry.RuleFor(e => e.ProductId).NotEmpty();
            entry.RuleFor(e => e.UnitPrice).GreaterThanOrEqualTo(0);
        });
    }
}
