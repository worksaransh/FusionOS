using FusionOS.SharedKernel;

namespace FusionOS.Modules.Sales.Domain.PortalUsers;

/// <summary>
/// A customer's own self-service login — a genuinely separate identity system
/// from Core's <c>User</c> (Core's staff/company-employee identity). A customer
/// contact is not a company employee and must never be able to accidentally
/// pick up RBAC/staff permissions, so this aggregate deliberately does not
/// reuse anything from Core.Application/Core.Infrastructure's Auth stack
/// (see PortalLoginCommandHandler's doc comment for the full reasoning) and
/// carries no permission/role concept at all — a portal token only ever proves
/// "I am this CustomerPortalUser, who belongs to this Customer."
///
/// CustomerId is a real, same-module foreign key (Customer lives in this
/// module too), validated the same way CreateSalesOrderCommandHandler
/// validates it via ICustomerRepository.ExistsAsync — see
/// CreateCustomerPortalUserCommandHandler.
///
/// Email uniqueness scope (design decision): unique per (CustomerId, Email)
/// rather than globally unique. A portal login's own email is only ever
/// meaningful as "this contact's login for this customer account" — two
/// unrelated Customers (potentially at two unrelated companies) may
/// legitimately share a contact mailbox (e.g. a shared "accounts@" inbox used
/// by a small business that is, coincidentally, a customer of two of our
/// tenants), and there is no product reason to force one of them to pick a
/// different email just because the other registered first. The trade-off
/// this creates - the same email can legitimately match more than one
/// CustomerPortalUser row - is handled explicitly in
/// PortalLoginCommandHandler by trying every candidate's password hash rather
/// than assuming email alone identifies a single row.
/// </summary>
public sealed class CustomerPortalUser : TenantAggregateRoot
{
    public Guid CustomerId { get; private set; }
    public string Email { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public bool IsActive { get; private set; } = true;

    private CustomerPortalUser() { }

    public static CustomerPortalUser Create(Guid companyId, Guid customerId, string email, string passwordHash)
    {
        if (customerId == Guid.Empty)
            throw new ArgumentException("Customer id is required.", nameof(customerId));
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new ArgumentException("A valid email is required.", nameof(email));
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash is required.", nameof(passwordHash));

        return new CustomerPortalUser
        {
            CompanyId = companyId,
            CustomerId = customerId,
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            IsActive = true,
        };
    }

    /// <summary>Locks the portal account out — reversible via Activate(), same shape as Customer.Deactivate().</summary>
    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;

    public void ChangePasswordHash(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new ArgumentException("Password hash is required.", nameof(newPasswordHash));

        PasswordHash = newPasswordHash;
    }
}
