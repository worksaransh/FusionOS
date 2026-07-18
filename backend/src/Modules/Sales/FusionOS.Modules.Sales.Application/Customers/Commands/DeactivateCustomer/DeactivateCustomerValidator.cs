using FluentValidation;

namespace FusionOS.Modules.Sales.Application.Customers.Commands.DeactivateCustomer;

public sealed class DeactivateCustomerValidator : AbstractValidator<DeactivateCustomerCommand>
{
    public DeactivateCustomerValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.CustomerId).NotEmpty();
    }
}
