using FluentValidation;

namespace FusionOS.Modules.Procurement.Application.SupplierContracts.Commands.CreateSupplierContract;

public sealed class CreateSupplierContractValidator : AbstractValidator<CreateSupplierContractCommand>
{
    public CreateSupplierContractValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.SupplierId).NotEmpty();
        RuleFor(x => x.Terms).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.EndDate).GreaterThan(x => x.StartDate).WithMessage("End date must be after the start date.");
    }
}
