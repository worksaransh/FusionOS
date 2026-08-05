using FusionOS.Modules.Billing.Domain.Plans;

namespace FusionOS.Modules.Billing.Application.Plans.Contracts;

public interface IPlanRepository
{
    Task AddAsync(Plan plan, CancellationToken cancellationToken = default);
    Task<Plan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Plan>> ListAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// "Seed on first use" — mirrors FusionOS.Modules.Core.Infrastructure.Repositories
    /// .UserRepository.GetOrCreateCompanyOwnerRoleAsync's own EnsurePermissionsSeededAsync
    /// pattern: if no Plan exists yet at all, a default "TRIAL" Plan is created before this
    /// returns. Backs BillingBootstrapService.EnsureDefaultSubscriptionAsync, which needs a
    /// real PlanId to point a brand-new company's default Subscription at even before an
    /// admin has ever created a Plan through the catalog CRUD.
    /// </summary>
    Task<Guid> GetOrCreateDefaultTrialPlanIdAsync(CancellationToken cancellationToken = default);
}
