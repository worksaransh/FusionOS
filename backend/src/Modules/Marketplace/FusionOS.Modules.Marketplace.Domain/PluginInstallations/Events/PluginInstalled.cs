using FusionOS.SharedKernel;

namespace FusionOS.Modules.Marketplace.Domain.PluginInstallations.Events;

/// <summary>Raised on install. No consumer this slice — the natural future hook once a real plugin runtime exists to actually activate/load code for this installation.</summary>
public sealed record PluginInstalled(Guid PluginInstallationId, Guid CompanyId, Guid PluginListingId) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
