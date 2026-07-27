using FusionOS.Modules.Ai.Domain.DemandHistory;

namespace FusionOS.Modules.Ai.Application.DemandHistory.Contracts;

public interface IDemandHistoryRepository
{
    Task AddAsync(DemandHistoryEntry entry, CancellationToken cancellationToken = default);

    /// <summary>Rows for one product within [periodStart, periodEnd], ordered by date ascending — the shape GetDemandForecastQueryHandler's regression/moving-average math expects.</summary>
    Task<IReadOnlyList<DemandHistoryEntry>> ListForProductAsync(Guid companyId, Guid productId, DateTimeOffset periodStart, DateTimeOffset periodEnd, CancellationToken cancellationToken = default);
}
