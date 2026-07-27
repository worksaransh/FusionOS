namespace FusionOS.Modules.Quality.Application.Inspections.Contracts;

public interface IInspectionRepository
{
    Task<Domain.Inspections.Inspection?> GetByIdAsync(Guid companyId, Guid inspectionId, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.Inspections.Inspection inspection, CancellationToken cancellationToken = default);

    /// <summary>Type is optional — omitted, this lists every inspection for the company regardless of InspectionType; supplied (e.g. Final), it scopes to just that type.</summary>
    Task<IReadOnlyList<Domain.Inspections.Inspection>> ListAsync(Guid companyId, Domain.Inspections.InspectionType? type, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, Domain.Inspections.InspectionType? type, CancellationToken cancellationToken = default);

    /// <summary>
    /// Every inspection for this company that has at least one item matching
    /// <paramref name="characteristic"/> (case-insensitive) with a measured value recorded,
    /// created within the optional period — backs GetSpcDataForCharacteristicQuery. Unbounded
    /// (no paging): SPC charting needs the full data series for the period, not a page of it.
    /// </summary>
    Task<IReadOnlyList<Domain.Inspections.Inspection>> ListByCharacteristicAsync(
        Guid companyId, string characteristic, DateTimeOffset? periodStart, DateTimeOffset? periodEnd, CancellationToken cancellationToken = default);
}
