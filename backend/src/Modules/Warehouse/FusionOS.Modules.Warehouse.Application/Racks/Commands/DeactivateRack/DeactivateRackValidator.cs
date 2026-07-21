using FluentValidation;

namespace FusionOS.Modules.Warehouse.Application.Racks.Commands.DeactivateRack;

public sealed class DeactivateRackValidator : AbstractValidator<DeactivateRackCommand>
{
    public DeactivateRackValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.Id).NotEmpty();
    }
}
