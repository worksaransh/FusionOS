using FluentValidation;

namespace FusionOS.Modules.Finance.Application.Payables.Commands.RecordBillCharge;

public sealed class RecordBillChargeValidator : AbstractValidator<RecordBillChargeCommand>
{
    public RecordBillChargeValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.SupplierId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0m).WithMessage("A bill charge amount must be greater than zero.");
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
    }
}
