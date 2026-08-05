using FluentValidation;

namespace FusionOS.Modules.BusinessIntelligence.Application.KpiDefinitions.Commands.UpdateKpiDefinition;

public sealed class UpdateKpiDefinitionValidator : AbstractValidator<UpdateKpiDefinitionCommand>
{
    public UpdateKpiDefinitionValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.KpiDefinitionId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Unit).MaximumLength(20);
    }
}
