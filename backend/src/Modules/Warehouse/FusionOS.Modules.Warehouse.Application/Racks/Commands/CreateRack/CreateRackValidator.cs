using FluentValidation;

namespace FusionOS.Modules.Warehouse.Application.Racks.Commands.CreateRack;

public sealed class CreateRackValidator : AbstractValidator<CreateRackCommand>
{
    public CreateRackValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.ZoneId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
    }
}
