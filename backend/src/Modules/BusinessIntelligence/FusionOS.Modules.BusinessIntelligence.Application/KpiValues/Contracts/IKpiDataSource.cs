namespace FusionOS.Modules.BusinessIntelligence.Application.KpiValues.Contracts;

/// <summary>
/// Phase 2.3 — one implementation per live-computable KPI type. Each data source
/// declares the KpiDefinition.Code it knows how to compute and reads the current
/// value straight from the owning module's existing report query via ISender —
/// the same cross-module Application-to-Application convention Ai's
/// GenerateKpiNarrativeReportQueryHandler established for calling into
/// BusinessIntelligence's own queries (see FusionOS.Modules.Ai.Application.csproj's
/// comment on the first cross-module Application references in this codebase).
/// This keeps BI a thin translation layer over already-authorized queries instead
/// of duplicating another module's ledger/report SQL, and requires zero new
/// tables — the "live" value is computed on read, never stored.
/// </summary>
public interface IKpiDataSource
{
    /// <summary>The KpiDefinition.Code (upper-invariant business key, e.g. "AR_OUTSTANDING") this data source can compute.</summary>
    string KpiCode { get; }

    Task<decimal> GetLiveValueAsync(Guid companyId, CancellationToken cancellationToken);
}
