using FluentValidation;

namespace FusionOS.Modules.IntegrationHub.Application.IntegrationConnectors.Commands.UpdateIntegrationConnector;

public sealed class UpdateIntegrationConnectorValidator : AbstractValidator<UpdateIntegrationConnectorCommand>
{
    public UpdateIntegrationConnectorValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.IntegrationConnectorId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Provider).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Category).IsInEnum();
    }
}
