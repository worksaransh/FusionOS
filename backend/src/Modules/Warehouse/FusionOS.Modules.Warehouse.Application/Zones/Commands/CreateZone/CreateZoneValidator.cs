using FluentValidation;

namespace FusionOS.Modules.Warehouse.Application.Zones.Commands.CreateZone;

public sealed class CreateZoneValidator : AbstractValidator<CreateZoneCommand>
{
    public CreateZoneValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.WarehouseId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
    }
}
