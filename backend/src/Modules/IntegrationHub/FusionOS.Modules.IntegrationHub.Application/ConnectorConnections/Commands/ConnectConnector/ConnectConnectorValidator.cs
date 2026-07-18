using FluentValidation;

namespace FusionOS.Modules.IntegrationHub.Application.ConnectorConnections.Commands.ConnectConnector;

public sealed class ConnectConnectorValidator : AbstractValidator<ConnectConnectorCommand>
{
    public ConnectConnectorValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.IntegrationConnectorId).NotEmpty();
        RuleFor(x => x.Label).NotEmpty().MaximumLength(200);
    }
}
