using FluentValidation;

namespace FusionOS.Modules.Warehouse.Application.Racks.Commands.UpdateRack;

public sealed class UpdateRackValidator : AbstractValidator<UpdateRackCommand>
{
    public UpdateRackValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
    }
}
