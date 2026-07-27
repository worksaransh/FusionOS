using FluentValidation;

namespace FusionOS.Modules.Sales.Application.Returns.Commands.RejectSalesReturn;

public sealed class RejectSalesReturnValidator : AbstractValidator<RejectSalesReturnCommand>
{
    public RejectSalesReturnValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.SalesReturnId).NotEmpty();
        RuleFor(x => x.RejectionReason).NotEmpty().MaximumLength(500).WithMessage("A rejection reason is required.");
    }
}
