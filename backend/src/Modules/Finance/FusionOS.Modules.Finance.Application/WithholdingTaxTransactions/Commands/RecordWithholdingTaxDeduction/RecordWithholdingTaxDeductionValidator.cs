using FluentValidation;

namespace FusionOS.Modules.Finance.Application.WithholdingTaxTransactions.Commands.RecordWithholdingTaxDeduction;

public sealed class RecordWithholdingTaxDeductionValidator : AbstractValidator<RecordWithholdingTaxDeductionCommand>
{
    public RecordWithholdingTaxDeductionValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.WithholdingTaxRateId).NotEmpty();
        RuleFor(x => x.ReferenceType).NotEmpty().MaximumLength(50);
        RuleFor(x => x.ReferenceId).NotEmpty();
        RuleFor(x => x.GrossAmount).GreaterThan(0);
        RuleFor(x => x.PayableGlAccountId).NotEmpty();
        RuleFor(x => x.WithholdingPayableGlAccountId).NotEmpty();
        RuleFor(x => x)
            .Must(x => x.PayableGlAccountId != x.WithholdingPayableGlAccountId)
            .WithMessage("Payable and withholding-payable GL accounts must differ.");
    }
}
