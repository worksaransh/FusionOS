using FluentValidation;

namespace FusionOS.Modules.IntegrationHub.Application.IntegrationConnectors.Commands.CreateIntegrationConnector;

public sealed class CreateIntegrationConnectorValidator : AbstractValidator<CreateIntegrationConnectorCommand>
{
    public CreateIntegrationConnectorValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Provider).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Category).IsInEnum();
    }
}
