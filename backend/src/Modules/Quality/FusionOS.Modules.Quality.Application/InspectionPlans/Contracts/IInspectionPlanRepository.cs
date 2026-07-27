using FusionOS.Modules.Quality.Domain.Inspections;

namespace FusionOS.Modules.Quality.Application.InspectionPlans.Contracts;

public interface IInspectionPlanRepository
{
    Task<Domain.InspectionPlans.InspectionPlan?> GetByIdAsync(Guid companyId, Guid inspectionPlanId, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.InspectionPlans.InspectionPlan plan, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.InspectionPlans.InspectionPlan>> ListAsync(Guid companyId, InspectionType? inspectionType, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, InspectionType? inspectionType, CancellationToken cancellationToken = default);
}
