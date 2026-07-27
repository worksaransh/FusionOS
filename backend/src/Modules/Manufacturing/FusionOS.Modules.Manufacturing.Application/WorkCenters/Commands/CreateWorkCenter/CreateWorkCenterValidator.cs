using FluentValidation;

namespace FusionOS.Modules.Manufacturing.Application.WorkCenters.Commands.CreateWorkCenter;

public sealed class CreateWorkCenterValidator : AbstractValidator<CreateWorkCenterCommand>
{
    public CreateWorkCenterValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CapacityHoursPerDay).GreaterThan(0);
    }
}
