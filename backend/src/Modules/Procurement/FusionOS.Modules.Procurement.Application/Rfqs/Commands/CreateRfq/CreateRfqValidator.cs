using FluentValidation;

namespace FusionOS.Modules.Procurement.Application.Rfqs.Commands.CreateRfq;

public sealed class CreateRfqValidator : AbstractValidator<CreateRfqCommand>
{
    public CreateRfqValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.Lines).NotEmpty().WithMessage("An RFQ must have at least one line.");
        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.ProductId).NotEmpty();
            line.RuleFor(l => l.Quantity).GreaterThan(0);
        });
    }
}
