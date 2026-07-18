using FusionOS.SharedKernel;

namespace FusionOS.Modules.Core.Domain.Settings;

/// <summary>
/// Per-company configuration (Phase M5, 2026-07-15 — Settings module, previously
/// 0% per docs/PROJECT_TRACKER.md). One row per CompanyId. There is
/// deliberately no explicit "Create" command for this entity: a company should
/// always have settings, even before anyone has ever opened this page, so
/// GetCompanySettingsQueryHandler creates a default row on first read
/// (get-or-create) — the same established pattern as
/// IUserRepository.GetOrCreateCompanyOwnerRoleAsync, just at the handler layer
/// instead of the repository layer since there's no other write path here.
///
/// Unlike every other Create factory in this codebase, CreateDefault raises no
/// domain event: nothing else in the system needs to react to a company's
/// settings existing (see docs/ORPHANED_EVENTS_AUDIT.md for the standing rule
/// this follows — don't publish an event with no real consumer just because
/// every other aggregate happens to raise one on creation).
/// </summary>
public sealed class CompanySettings : TenantAggregateRoot
{
    public string DefaultCurrency { get; private set; } = "USD";
    public int DefaultPageSize { get; private set; } = 25;
    public string? DisplayName { get; private set; }
    public string? LogoUrl { get; private set; }

    private CompanySettings() { }

    public static CompanySettings CreateDefault(Guid companyId)
    {
        if (companyId == Guid.Empty)
            throw new ArgumentException("Company id is required.", nameof(companyId));

        return new CompanySettings
        {
            CompanyId = companyId,
            DefaultCurrency = "USD",
            DefaultPageSize = 25,
        };
    }

    public void UpdateSettings(string defaultCurrency, int defaultPageSize, string? displayName, string? logoUrl)
    {
        if (string.IsNullOrWhiteSpace(defaultCurrency) || defaultCurrency.Length != 3)
            throw new ArgumentException("Default currency must be a 3-letter ISO 4217 code.", nameof(defaultCurrency));
        if (defaultPageSize is < 1 or > 200)
            throw new ArgumentException("Default page size must be between 1 and 200.", nameof(defaultPageSize));

        DefaultCurrency = defaultCurrency.ToUpperInvariant();
        DefaultPageSize = defaultPageSize;
        DisplayName = string.IsNullOrWhiteSpace(displayName) ? null : displayName.Trim();
        LogoUrl = string.IsNullOrWhiteSpace(logoUrl) ? null : logoUrl.Trim();
    }
}
