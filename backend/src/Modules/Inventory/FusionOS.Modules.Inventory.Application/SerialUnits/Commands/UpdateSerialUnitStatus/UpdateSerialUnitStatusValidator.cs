using FluentValidation;
using FusionOS.Modules.Inventory.Domain.SerialUnits;

namespace FusionOS.Modules.Inventory.Application.SerialUnits.Commands.UpdateSerialUnitStatus;

public sealed class UpdateSerialUnitStatusValidator : AbstractValidator<UpdateSerialUnitStatusCommand>
{
    public UpdateSerialUnitStatusValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.SerialUnitId).NotEmpty();
        RuleFor(x => x.NewStatus)
            .IsInEnum()
            .NotEqual(SerialUnitStatus.InStock)
            .WithMessage("InStock is only set at registration — it is not a valid target status for this command.");
    }
}
