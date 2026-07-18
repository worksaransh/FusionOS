using FluentValidation;

namespace FusionOS.Modules.Finance.Application.Payables.Commands.RecordPayment;

public sealed class RecordPaymentValidator : AbstractValidator<RecordPaymentCommand>
{
    public RecordPaymentValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.SupplierId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0m).WithMessage("A payment amount must be greater than zero.");
    }
}
