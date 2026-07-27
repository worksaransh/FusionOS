namespace FusionOS.Modules.Maintenance.Application.MeterReadings.Contracts;

public interface IMeterReadingRepository
{
    Task AddAsync(Domain.MeterReadings.MeterReading reading, CancellationToken cancellationToken = default);

    /// <summary>The asset's most recent reading by ReadingDate (ties broken by CreatedAt) — backs both GetLatestMeterReadingForAssetQuery and RecordMeterReadingCommandHandler's monotonically-increasing check. Null when the asset has no readings yet.</summary>
    Task<Domain.MeterReadings.MeterReading?> GetLatestForAssetAsync(Guid companyId, Guid assetId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Domain.MeterReadings.MeterReading>> ListForAssetAsync(Guid companyId, Guid assetId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountForAssetAsync(Guid companyId, Guid assetId, CancellationToken cancellationToken = default);
}
