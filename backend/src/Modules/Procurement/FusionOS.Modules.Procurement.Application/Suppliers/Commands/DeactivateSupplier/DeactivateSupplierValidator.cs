using FluentValidation;

namespace FusionOS.Modules.Procurement.Application.Suppliers.Commands.DeactivateSupplier;

public sealed class DeactivateSupplierValidator : AbstractValidator<DeactivateSupplierCommand>
{
    public DeactivateSupplierValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.SupplierId).NotEmpty();
    }
}
