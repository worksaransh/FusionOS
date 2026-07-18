using FluentValidation;

namespace FusionOS.Modules.Finance.Application.CostCenters.Commands.CreateCostCenter;

public sealed class CreateCostCenterValidator : AbstractValidator<CreateCostCenterCommand>
{
    public CreateCostCenterValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}
