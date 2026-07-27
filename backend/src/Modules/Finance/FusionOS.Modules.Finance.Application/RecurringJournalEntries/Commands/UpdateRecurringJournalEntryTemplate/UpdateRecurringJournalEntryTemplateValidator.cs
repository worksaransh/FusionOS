using FluentValidation;

namespace FusionOS.Modules.Finance.Application.RecurringJournalEntries.Commands.UpdateRecurringJournalEntryTemplate;

public sealed class UpdateRecurringJournalEntryTemplateValidator : AbstractValidator<UpdateRecurringJournalEntryTemplateCommand>
{
    public UpdateRecurringJournalEntryTemplateValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.RecurringJournalEntryTemplateId).NotEmpty();
        RuleFor(x => x.Reference).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Frequency).IsInEnum();
        RuleFor(x => x.NextDueDate).NotEmpty();
        RuleFor(x => x.Lines).Must(l => l.Count >= 2).WithMessage("A recurring journal entry template must have at least two lines.");
        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.AccountId).NotEmpty();
            line.RuleFor(l => l.Debit).GreaterThanOrEqualTo(0);
            line.RuleFor(l => l.Credit).GreaterThanOrEqualTo(0);
            line.RuleFor(l => l).Must(l => (l.Debit > 0) != (l.Credit > 0))
                .WithMessage("Each line must have exactly one of debit or credit greater than zero.");
        });
        RuleFor(x => x).Must(x => x.Lines.Sum(l => l.Debit) == x.Lines.Sum(l => l.Credit))
            .WithMessage("Total debit must equal total credit.");
    }
}
