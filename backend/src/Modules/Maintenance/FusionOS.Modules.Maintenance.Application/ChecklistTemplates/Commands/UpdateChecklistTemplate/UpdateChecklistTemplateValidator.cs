using FluentValidation;

namespace FusionOS.Modules.Maintenance.Application.ChecklistTemplates.Commands.UpdateChecklistTemplate;

public sealed class UpdateChecklistTemplateValidator : AbstractValidator<UpdateChecklistTemplateCommand>
{
    public UpdateChecklistTemplateValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.ChecklistTemplateId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.StepDescriptions).NotEmpty();
        RuleForEach(x => x.StepDescriptions).NotEmpty().MaximumLength(500);
    }
}
