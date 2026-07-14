using FluentValidation;

namespace FusionOS.Modules.Procurement.Application.Suppliers.Commands.CreateSupplier;

public sealed class CreateSupplierValidator : AbstractValidator<CreateSupplierCommand>
{
    public CreateSupplierValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(30);
        RuleFor(x => x.ContactEmail).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.ContactEmail));
        RuleFor(x => x.ContactPhone).MaximumLength(30);
    }
}
