using FusionOS.Modules.Ai.Domain.OcrJobs;

namespace FusionOS.Modules.Ai.Application.OcrJobs.Contracts;

public interface IOcrJobRepository
{
    Task<OcrJob?> GetByIdAsync(Guid companyId, Guid ocrJobId, CancellationToken cancellationToken = default);
    Task AddAsync(OcrJob job, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OcrJob>> ListAsync(Guid companyId, OcrJobStatus? status, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, OcrJobStatus? status, CancellationToken cancellationToken = default);
}
