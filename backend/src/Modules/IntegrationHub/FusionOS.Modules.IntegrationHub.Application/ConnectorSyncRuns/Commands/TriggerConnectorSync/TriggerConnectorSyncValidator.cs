using FluentValidation;

namespace FusionOS.Modules.IntegrationHub.Application.ConnectorSyncRuns.Commands.TriggerConnectorSync;

public sealed class TriggerConnectorSyncValidator : AbstractValidator<TriggerConnectorSyncCommand>
{
    public TriggerConnectorSyncValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.ConnectionId).NotEmpty();
    }
}
