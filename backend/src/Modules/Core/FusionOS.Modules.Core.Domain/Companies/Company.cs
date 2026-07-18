using FusionOS.SharedKernel;
using FusionOS.Modules.Core.Domain.Companies.Events;

namespace FusionOS.Modules.Core.Domain.Companies;

/// <summary>
/// A Company is the tenant root (04_DATABASE_GUIDELINES.md §6). It is the one
/// documented, reviewed exception to "every table has a CompanyId" — a company
/// does not belong to another company.
/// </summary>
public sealed class Company : AuditableAggregateRoot
{
    public string Name { get; private set; } = default!;
    public string LegalName { get; private set; } = default!;
    public string? TaxId { get; private set; }
    public string BaseCurrency { get; private set; } = "USD";
    public bool IsActive { get; private set; } = true;

    private Company() { }

    public static Company Create(string name, string legalName, string baseCurrency, string? taxId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Company name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(legalName))
            throw new ArgumentException("Company legal name is required.", nameof(legalName));
        if (string.IsNullOrWhiteSpace(baseCurrency) || baseCurrency.Length != 3)
            throw new ArgumentException("Base currency must be a 3-letter ISO 4217 code.", nameof(baseCurrency));

        var company = new Company
        {
            Name = name.Trim(),
            LegalName = legalName.Trim(),
            BaseCurrency = baseCurrency.ToUpperInvariant(),
            TaxId = taxId,
        };

        company.Raise(new CompanyCreated(company.Id, company.Name));
        return company;
    }

    public void Deactivate() => IsActive = false;

    /// <summary>Renames/updates the company's own details (Phase I) — same validation style as Create.</summary>
    public void UpdateDetails(string name, string legalName, string? taxId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Company name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(legalName))
            throw new ArgumentException("Company legal name is required.", nameof(legalName));

        Name = name.Trim();
        LegalName = legalName.Trim();
        TaxId = taxId;
    }
}
