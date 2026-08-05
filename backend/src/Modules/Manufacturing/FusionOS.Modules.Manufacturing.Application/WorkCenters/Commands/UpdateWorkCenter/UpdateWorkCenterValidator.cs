using FluentValidation;

namespace FusionOS.Modules.Manufacturing.Application.WorkCenters.Commands.UpdateWorkCenter;

public sealed class UpdateWorkCenterValidator : AbstractValidator<UpdateWorkCenterCommand>
{
    public UpdateWorkCenterValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.WorkCenterId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CapacityHoursPerDay).GreaterThan(0);
    }
}
