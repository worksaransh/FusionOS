using FluentValidation;

namespace FusionOS.Modules.Finance.Application.IntercompanyEliminationRules.Commands.CreateIntercompanyEliminationRule;

public sealed class CreateIntercompanyEliminationRuleValidator : AbstractValidator<CreateIntercompanyEliminationRuleCommand>
{
    public CreateIntercompanyEliminationRuleValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.ReceivableAccountCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.PayableAccountCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(200);
        RuleFor(x => x)
            .Must(x => !string.Equals(x.ReceivableAccountCode?.Trim(), x.PayableAccountCode?.Trim(), StringComparison.OrdinalIgnoreCase))
            .WithMessage("Receivable and payable account codes must differ.");
    }
}
