using FluentValidation;

namespace FusionOS.Modules.Inventory.Application.Attributes.Commands.DeactivateAttributeDefinition;

public sealed class DeactivateAttributeDefinitionValidator : AbstractValidator<DeactivateAttributeDefinitionCommand>
{
    public DeactivateAttributeDefinitionValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.AttributeDefinitionId).NotEmpty();
    }
}
