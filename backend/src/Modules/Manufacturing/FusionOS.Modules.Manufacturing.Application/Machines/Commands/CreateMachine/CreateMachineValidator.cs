using FluentValidation;

namespace FusionOS.Modules.Manufacturing.Application.Machines.Commands.CreateMachine;

public sealed class CreateMachineValidator : AbstractValidator<CreateMachineCommand>
{
    public CreateMachineValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.WorkCenterId).NotEmpty();
    }
}
