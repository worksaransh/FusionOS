using FluentValidation;

namespace FusionOS.Modules.IntegrationHub.Application.ConnectorConnections.Commands.ConfigureConnectorCredential;

public sealed class ConfigureConnectorCredentialValidator : AbstractValidator<ConfigureConnectorCredentialCommand>
{
    public ConfigureConnectorCredentialValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.ConnectionId).NotEmpty();
        RuleFor(x => x.CredentialReference).NotEmpty().MaximumLength(200);
    }
}
