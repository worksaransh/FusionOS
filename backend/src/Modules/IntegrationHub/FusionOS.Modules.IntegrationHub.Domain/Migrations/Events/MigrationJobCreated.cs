using FusionOS.SharedKernel;

namespace FusionOS.Modules.IntegrationHub.Domain.Migrations.Events;

/// <summary>Raised on MigrationJob creation. No consumer this slice — same deliberate restraint as IntegrationConnectorCreated.</summary>
public sealed record MigrationJobCreated(Guid MigrationJobId, Guid CompanyId, string SourceSystem) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
