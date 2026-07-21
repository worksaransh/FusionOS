using FluentValidation;

namespace FusionOS.Modules.Inventory.Application.Attributes.Commands.CreateAttributeValue;

public sealed class CreateAttributeValueValidator : AbstractValidator<CreateAttributeValueCommand>
{
    public CreateAttributeValueValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.AttributeDefinitionId).NotEmpty();
        RuleFor(x => x.Value).NotEmpty().MaximumLength(100);
    }
}
