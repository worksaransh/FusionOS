using FluentValidation;

namespace FusionOS.Modules.Warehouse.Application.Zones.Commands.DeactivateZone;

public sealed class DeactivateZoneValidator : AbstractValidator<DeactivateZoneCommand>
{
    public DeactivateZoneValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.Id).NotEmpty();
    }
}
