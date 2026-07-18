using FluentValidation;

namespace FusionOS.Modules.BusinessIntelligence.Application.KpiDefinitions.Commands.CreateKpiDefinition;

public sealed class CreateKpiDefinitionValidator : AbstractValidator<CreateKpiDefinitionCommand>
{
    public CreateKpiDefinitionValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Unit).MaximumLength(20);
    }
}
