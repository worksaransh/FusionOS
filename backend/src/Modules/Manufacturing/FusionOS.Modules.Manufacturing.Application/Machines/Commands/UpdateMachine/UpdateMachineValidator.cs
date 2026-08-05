using FluentValidation;

namespace FusionOS.Modules.Manufacturing.Application.Machines.Commands.UpdateMachine;

public sealed class UpdateMachineValidator : AbstractValidator<UpdateMachineCommand>
{
    public UpdateMachineValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.MachineId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.WorkCenterId).NotEmpty();
    }
}
