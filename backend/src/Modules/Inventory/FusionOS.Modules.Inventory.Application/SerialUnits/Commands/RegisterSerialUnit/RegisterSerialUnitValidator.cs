using FluentValidation;

namespace FusionOS.Modules.Inventory.Application.SerialUnits.Commands.RegisterSerialUnit;

public sealed class RegisterSerialUnitValidator : AbstractValidator<RegisterSerialUnitCommand>
{
    public RegisterSerialUnitValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.SerialNumber).NotEmpty().MaximumLength(100);
    }
}
