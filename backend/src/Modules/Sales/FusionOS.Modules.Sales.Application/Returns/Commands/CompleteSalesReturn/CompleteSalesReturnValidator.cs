using FluentValidation;

namespace FusionOS.Modules.Sales.Application.Returns.Commands.CompleteSalesReturn;

public sealed class CompleteSalesReturnValidator : AbstractValidator<CompleteSalesReturnCommand>
{
    public CompleteSalesReturnValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.SalesReturnId).NotEmpty();
        RuleFor(x => x.LineCompletions).NotEmpty().WithMessage("Every line of the sales return must have a completion entry.");
        RuleForEach(x => x.LineCompletions).ChildRules(line =>
        {
            line.RuleFor(l => l.LineId).NotEmpty();
            line.RuleFor(l => l.ConditionNotes).MaximumLength(500);
        });
    }
}
