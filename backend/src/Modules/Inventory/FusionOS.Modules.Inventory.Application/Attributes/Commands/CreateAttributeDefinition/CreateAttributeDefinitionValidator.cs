using FluentValidation;

namespace FusionOS.Modules.Inventory.Application.Attributes.Commands.CreateAttributeDefinition;

public sealed class CreateAttributeDefinitionValidator : AbstractValidator<CreateAttributeDefinitionCommand>
{
    public CreateAttributeDefinitionValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}
