using FluentValidation;

namespace FusionOS.Modules.Finance.Application.CostCenters.Commands.UpdateCostCenter;

public sealed class UpdateCostCenterValidator : AbstractValidator<UpdateCostCenterCommand>
{
    public UpdateCostCenterValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.CostCenterId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}
