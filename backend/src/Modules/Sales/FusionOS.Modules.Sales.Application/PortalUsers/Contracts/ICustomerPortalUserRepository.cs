namespace FusionOS.Modules.Sales.Application.PortalUsers.Contracts;

public interface ICustomerPortalUserRepository
{
    Task<Domain.PortalUsers.CustomerPortalUser?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Every row matching this email, regardless of which Customer it belongs
    /// to or whether it is active - deliberately NOT scoped to a single
    /// Customer, since login happens before we know which Customer the caller
    /// belongs to. Email is only unique per-Customer (CustomerPortalUser's own
    /// doc comment), so more than one row can come back; PortalLoginCommandHandler
    /// is responsible for finding the one whose password hash matches.
    /// </summary>
    Task<IReadOnlyList<Domain.PortalUsers.CustomerPortalUser>> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<bool> ExistsForCustomerAsync(Guid customerId, string email, CancellationToken cancellationToken = default);

    Task AddAsync(Domain.PortalUsers.CustomerPortalUser portalUser, CancellationToken cancellationToken = default);
}
