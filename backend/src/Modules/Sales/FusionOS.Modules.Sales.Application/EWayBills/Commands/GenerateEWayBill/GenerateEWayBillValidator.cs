using FluentValidation;

namespace FusionOS.Modules.Sales.Application.EWayBills.Commands.GenerateEWayBill;

public sealed class GenerateEWayBillValidator : AbstractValidator<GenerateEWayBillCommand>
{
    public GenerateEWayBillValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.DispatchId).NotEmpty();
    }
}
