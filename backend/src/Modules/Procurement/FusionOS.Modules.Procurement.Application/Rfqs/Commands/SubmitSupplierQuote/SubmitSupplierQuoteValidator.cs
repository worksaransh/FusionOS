using FluentValidation;

namespace FusionOS.Modules.Procurement.Application.Rfqs.Commands.SubmitSupplierQuote;

public sealed class SubmitSupplierQuoteValidator : AbstractValidator<SubmitSupplierQuoteCommand>
{
    public SubmitSupplierQuoteValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.RfqId).NotEmpty();
        RuleFor(x => x.SupplierId).NotEmpty();
        RuleFor(x => x.Lines).NotEmpty().WithMessage("A supplier quote must have at least one line.");
        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.ProductId).NotEmpty();
            line.RuleFor(l => l.Quantity).GreaterThan(0);
            line.RuleFor(l => l.UnitPrice).GreaterThanOrEqualTo(0);
        });
    }
}
