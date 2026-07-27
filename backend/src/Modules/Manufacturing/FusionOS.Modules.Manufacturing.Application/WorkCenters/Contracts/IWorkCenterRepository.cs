namespace FusionOS.Modules.Manufacturing.Application.WorkCenters.Contracts;

public interface IWorkCenterRepository
{
    Task<bool> CodeExistsAsync(Guid companyId, string code, CancellationToken cancellationToken = default);
    Task<Domain.WorkCenters.WorkCenter?> GetByIdAsync(Guid companyId, Guid workCenterId, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.WorkCenters.WorkCenter workCenter, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.WorkCenters.WorkCenter>> ListAsync(Guid companyId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, CancellationToken cancellationToken = default);

    /// <summary>Every active work center for the company, unpaged — backs GetWorkCenterCapacityLoadQuery and the machine-creation picker, same "whole active register in one shot" pattern as Finance's ITaxRateRepository.ListAllActiveAsync.</summary>
    Task<IReadOnlyList<Domain.WorkCenters.WorkCenter>> ListAllActiveAsync(Guid companyId, CancellationToken cancellationToken = default);
}
