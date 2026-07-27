namespace FusionOS.Modules.Quality.Application.Calibrations.Contracts;

public interface ICalibrationRecordRepository
{
    Task<Domain.Calibrations.CalibrationRecord?> GetByIdAsync(Guid companyId, Guid calibrationRecordId, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.Calibrations.CalibrationRecord record, CancellationToken cancellationToken = default);

    /// <summary>overdueOnly=true scopes to records whose NextCalibrationDueDate is before now; null/false lists every record.</summary>
    Task<IReadOnlyList<Domain.Calibrations.CalibrationRecord>> ListAsync(Guid companyId, bool overdueOnly, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, bool overdueOnly, CancellationToken cancellationToken = default);
}
