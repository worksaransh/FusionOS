using FluentValidation;

namespace FusionOS.Modules.Manufacturing.Application.WorkCenters.Commands.DeactivateWorkCenter;

public sealed class DeactivateWorkCenterValidator : AbstractValidator<DeactivateWorkCenterCommand>
{
    public DeactivateWorkCenterValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.WorkCenterId).NotEmpty();
    }
}
