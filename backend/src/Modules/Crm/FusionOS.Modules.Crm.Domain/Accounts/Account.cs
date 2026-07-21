using FusionOS.SharedKernel;
using FusionOS.Modules.Crm.Domain.Accounts.Events;

namespace FusionOS.Modules.Crm.Domain.Accounts;

/// <summary>
/// Phase 4 — CRM. The organization/company a Lead, Opportunity, or Contact belongs
/// to — a pre-sales concept, distinct from Sales' <c>Customer</c> (which only exists
/// once an Opportunity is won). Leads, Opportunities, and Contacts each hold their
/// own nullable <c>AccountId</c> reference back to this aggregate (same same-module,
/// reference-by-id convention Opportunity already uses for <c>LeadId</c>) — this
/// aggregate itself owns no collections of them, so winning/losing/deleting an
/// Opportunity or Lead never touches this row.
/// </summary>
public sealed class Account : TenantAggregateRoot
{
    public string Name { get; private set; } = default!;
    public string? Industry { get; private set; }
    public string? Website { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Account() { }

    public static Account Create(Guid companyId, string name, string? industry, string? website)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Account name is required.", nameof(name));

        var account = new Account
        {
            CompanyId = companyId,
            Name = name.Trim(),
            Industry = string.IsNullOrWhiteSpace(industry) ? null : industry.Trim(),
            Website = string.IsNullOrWhiteSpace(website) ? null : website.Trim(),
        };

        account.Raise(new AccountCreated(account.Id, companyId, account.Name));
        return account;
    }

    public void UpdateDetails(string name, string? industry, string? website)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Account name is required.", nameof(name));

        Name = name.Trim();
        Industry = string.IsNullOrWhiteSpace(industry) ? null : industry.Trim();
        Website = string.IsNullOrWhiteSpace(website) ? null : website.Trim();
    }

    /// <summary>Soft-deactivate only — never a hard delete (same convention as Customer.Deactivate).</summary>
    public void Deactivate() => IsActive = false;
}
