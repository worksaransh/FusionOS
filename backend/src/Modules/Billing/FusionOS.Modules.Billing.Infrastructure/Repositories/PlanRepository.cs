using FusionOS.Modules.Billing.Application.Plans.Contracts;
using FusionOS.Modules.Billing.Domain.Plans;
using FusionOS.Modules.Billing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Billing.Infrastructure.Repositories;

public sealed class PlanRepository : IPlanRepository
{
    private const string DefaultTrialPlanCode = "TRIAL";

    private readonly BillingDbContext _context;

    public PlanRepository(BillingDbContext context) => _context = context;

    public async Task AddAsync(Plan plan, CancellationToken cancellationToken = default) =>
        await _context.Plans.AddAsync(plan, cancellationToken);

    public Task<Plan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Plans.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default) =>
        _context.Plans.AnyAsync(p => p.Code == code.Trim().ToUpperInvariant(), cancellationToken);

    public async Task<IReadOnlyList<Plan>> ListAsync(int page, int pageSize, CancellationToken cancellationToken = default) =>
        await _context.Plans
            .OrderBy(p => p.Tier)
            .ThenBy(p => p.Code)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(CancellationToken cancellationToken = default) =>
        _context.Plans.CountAsync(cancellationToken);

    /// <summary>
    /// "Seed on first use" — mirrors UserRepository.EnsurePermissionsSeededAsync's own
    /// check-then-create pattern (FusionOS.Modules.Core.Infrastructure.Repositories
    /// .UserRepository), just scoped to a single default row instead of a whole catalog.
    ///
    /// IncludedModules lists every module (see PermissionCatalog.cs module prefixes) so a
    /// brand-new signup's 14-day trial is fully-featured, not empty — PlanEntitlementBehavior
    /// blocks any module absent from this list, and an empty list here would 403 a fresh
    /// company out of every module except Core the moment it registers.
    /// </summary>
    private const string AllModules = "core,finance,inventory,procurement,sales,warehouse,manufacturing,crm,hrms,quality,maintenance,bi,ai,marketplace,billing";

    public async Task<Guid> GetOrCreateDefaultTrialPlanIdAsync(CancellationToken cancellationToken = default)
    {
        var existing = await _context.Plans.FirstOrDefaultAsync(p => p.Code == DefaultTrialPlanCode, cancellationToken);
        if (existing is not null)
            return existing.Id;

        var plan = Plan.Create(DefaultTrialPlanCode, "Trial", PlanTier.Starter, monthlyPricePerUser: 0m, includedModules: AllModules);
        await _context.Plans.AddAsync(plan, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return plan.Id;
    }
}
