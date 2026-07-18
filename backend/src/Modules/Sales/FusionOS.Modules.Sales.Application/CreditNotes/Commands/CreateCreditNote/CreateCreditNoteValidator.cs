using FluentValidation;

namespace FusionOS.Modules.Sales.Application.CreditNotes.Commands.CreateCreditNote;

public sealed class CreateCreditNoteValidator : AbstractValidator<CreateCreditNoteCommand>
{
    public CreateCreditNoteValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.InvoiceId).NotEmpty();
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Lines).NotEmpty().WithMessage("A credit note must have at least one line.");
        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.ProductId).NotEmpty();
            line.RuleFor(l => l.Quantity).GreaterThan(0);
            line.RuleFor(l => l.UnitPrice).GreaterThanOrEqualTo(0);
        });
    }
}
