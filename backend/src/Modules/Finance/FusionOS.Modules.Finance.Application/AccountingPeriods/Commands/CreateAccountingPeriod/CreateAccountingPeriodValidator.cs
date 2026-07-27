using FluentValidation;

namespace FusionOS.Modules.Finance.Application.AccountingPeriods.Commands.CreateAccountingPeriod;

public sealed class CreateAccountingPeriodValidator : AbstractValidator<CreateAccountingPeriodCommand>
{
    public CreateAccountingPeriodValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.PeriodStart).NotEmpty();
        RuleFor(x => x.PeriodEnd).NotEmpty().GreaterThan(x => x.PeriodStart)
            .WithMessage("Period end must be after period start.");
    }
}
