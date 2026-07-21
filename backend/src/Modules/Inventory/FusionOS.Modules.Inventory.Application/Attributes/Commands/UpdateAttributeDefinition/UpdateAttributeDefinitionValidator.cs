using FluentValidation;

namespace FusionOS.Modules.Inventory.Application.Attributes.Commands.UpdateAttributeDefinition;

public sealed class UpdateAttributeDefinitionValidator : AbstractValidator<UpdateAttributeDefinitionCommand>
{
    public UpdateAttributeDefinitionValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.AttributeDefinitionId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}
