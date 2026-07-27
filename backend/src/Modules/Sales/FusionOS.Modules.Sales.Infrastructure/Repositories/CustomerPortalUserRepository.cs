using FusionOS.Modules.Sales.Application.PortalUsers.Contracts;
using FusionOS.Modules.Sales.Domain.PortalUsers;
using FusionOS.Modules.Sales.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Sales.Infrastructure.Repositories;

public sealed class CustomerPortalUserRepository : ICustomerPortalUserRepository
{
    private readonly SalesDbContext _context;

    // Deliberately Set<CustomerPortalUser>() rather than a named
    // SalesDbContext.CustomerPortalUsers DbSet property - SalesDbContext.cs is
    // off-limits for this task (other agents are editing it concurrently).
    // CustomerPortalUserConfiguration is still picked up by
    // modelBuilder.ApplyConfigurationsFromAssembly (SalesDbContext.OnModelCreating),
    // which is sufficient on its own to register the entity into the EF model
    // even with no DbSet property, so this works today without that edit.
    // Recommended (not required) follow-up for consistency with every other
    // aggregate: add `public DbSet<Domain.PortalUsers.CustomerPortalUser> CustomerPortalUsers => Set<Domain.PortalUsers.CustomerPortalUser>();`
    // to SalesDbContext - see this task's report for the exact line.
    private DbSet<CustomerPortalUser> PortalUsers => _context.Set<CustomerPortalUser>();

    public CustomerPortalUserRepository(SalesDbContext context) => _context = context;

    public Task<CustomerPortalUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        PortalUsers.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    // Deliberately not scoped by CompanyId/CustomerId - login happens before
    // we know which customer the caller belongs to. Normalizes to the same
    // lower-cased form CustomerPortalUser.Create stores.
    public async Task<IReadOnlyList<CustomerPortalUser>> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        await PortalUsers
            .Where(x => x.Email == email.Trim().ToLower())
            .ToListAsync(cancellationToken);

    public Task<bool> ExistsForCustomerAsync(Guid customerId, string email, CancellationToken cancellationToken = default) =>
        PortalUsers.AnyAsync(x => x.CustomerId == customerId && x.Email == email.Trim().ToLower(), cancellationToken);

    public async Task AddAsync(CustomerPortalUser portalUser, CancellationToken cancellationToken = default) =>
        await PortalUsers.AddAsync(portalUser, cancellationToken);
}
