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

    /// <summary>
    /// Self-referencing FK to another Company (Phase 2.4 — multi-entity/subsidiary
    /// consolidation). Null means "this company has no parent" — the ordinary,
    /// single-company case that must keep working undisturbed. When set, this
    /// company is a subsidiary consolidated under the referenced parent company
    /// for reporting purposes (see GetConsolidatedTrialBalanceQuery). Deliberately
    /// shallow (no depth/cycle validation here) — Phase 2.4 only models one level
    /// of parent/subsidiary, matching the roadmap's scope.
    /// </summary>
    public Guid? ParentCompanyId { get; private set; }

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

    /// <summary>
    /// Sets or clears this company's parent for consolidation purposes (Phase 2.4).
    /// Pass null to detach from any parent. Guards against the one structurally
    /// invalid case that would otherwise be silently accepted: a company naming
    /// itself as its own parent. Cross-company reachability (e.g. A -&gt; B -&gt; A cycles)
    /// is intentionally not checked here — the roadmap scope for this phase is a
    /// single parent/subsidiary level, and enforcing deeper-cycle detection would
    /// require a company directory lookup this aggregate does not have.
    /// </summary>
    public void SetParentCompany(Guid? parentCompanyId)
    {
        if (parentCompanyId is { } id && id == Id)
            throw new ArgumentException("A company cannot be its own parent.", nameof(parentCompanyId));

        ParentCompanyId = parentCompanyId;
    }
}
