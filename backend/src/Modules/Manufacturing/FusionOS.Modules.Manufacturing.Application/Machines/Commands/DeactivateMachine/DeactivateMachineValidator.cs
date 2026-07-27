using FluentValidation;

namespace FusionOS.Modules.Manufacturing.Application.Machines.Commands.DeactivateMachine;

public sealed class DeactivateMachineValidator : AbstractValidator<DeactivateMachineCommand>
{
    public DeactivateMachineValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.MachineId).NotEmpty();
    }
}
