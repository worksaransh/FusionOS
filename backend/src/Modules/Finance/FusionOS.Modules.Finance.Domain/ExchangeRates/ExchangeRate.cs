using FusionOS.SharedKernel;
using FusionOS.Modules.Finance.Domain.ExchangeRates.Events;

namespace FusionOS.Modules.Finance.Domain.ExchangeRates;

/// <summary>
/// M8e — Finance depth: multi-currency support, the fifth of the Phase M8
/// a–h sub-slices. A dated FX rate between two ISO 4217 currency codes: "1
/// FromCurrencyCode = Rate ToCurrencyCode" as of EffectiveDate — e.g.
/// From="USD", To="EUR", Rate=0.92, EffectiveDate=2026-07-17 means 1 USD
/// converts to 0.92 EUR on/after that date (until a newer dated rate for the
/// same pair supersedes it as "latest").
///
/// <b>Scope decision (Phase M8e, 2026-07-17), documented here the same way
/// CostCenter's and TaxRate's own class doc comments document their
/// scope-outs:</b> this is master-data-plus-a-conversion-utility only. No
/// existing aggregate — Account, JournalEntry (and JournalEntryLine),
/// Invoice, PurchaseOrder, ArLedgerEntry, ApLedgerEntry, or BankAccount — has
/// been modified to carry a CurrencyCode field, and nothing anywhere
/// automatically converts a posted amount through this rate. What IS in
/// scope: the ability to record/look up/manage dated rates for a currency
/// pair, and a pure, on-demand conversion query
/// (ConvertAmountQuery/Handler) that looks up the latest applicable rate and
/// does the arithmetic. Wiring an optional CurrencyCode onto every
/// transactional line, plus revaluation and realized/unrealized FX
/// gain/loss postings, is a distinct, separately-scoped, materially larger
/// future phase — exactly the same "master data now, wiring onto
/// transactions later" split M8a took for CostCenter/JournalEntryLine and
/// M8b took for TaxRate/any transactional line.
///
/// Currency codes are validated for shape only (exactly 3 letters,
/// normalized uppercase, e.g. "USD") — there is no ISO 4217 lookup table
/// enforcing that the code is a real, currently-circulating currency, the
/// same "domain enforces shape, not an external registry" stance
/// BankAccount.cs takes on AccountNumberLast4 not being checksummed against
/// any real bank.
/// </summary>
public sealed class ExchangeRate : TenantAggregateRoot
{
    public string FromCurrencyCode { get; private set; } = default!;
    public string ToCurrencyCode { get; private set; } = default!;
    public decimal Rate { get; private set; }
    public DateTimeOffset EffectiveDate { get; private set; }
    public bool IsActive { get; private set; } = true;

    private ExchangeRate() { }

    public static ExchangeRate Create(Guid companyId, string fromCurrencyCode, string toCurrencyCode, decimal rate, DateTimeOffset effectiveDate)
    {
        var normalizedFrom = NormalizeCurrencyCode(fromCurrencyCode, nameof(fromCurrencyCode));
        var normalizedTo = NormalizeCurrencyCode(toCurrencyCode, nameof(toCurrencyCode));

        if (normalizedFrom == normalizedTo)
            throw new ArgumentException("From and To currency codes must differ — converting a currency to itself is a data-entry error, not a real rate.", nameof(toCurrencyCode));
        if (rate <= 0)
            throw new ArgumentException("Exchange rate must be greater than zero.", nameof(rate));

        var exchangeRate = new ExchangeRate
        {
            CompanyId = companyId,
            FromCurrencyCode = normalizedFrom,
            ToCurrencyCode = normalizedTo,
            Rate = rate,
            EffectiveDate = effectiveDate,
        };

        exchangeRate.Raise(new ExchangeRateCreated(exchangeRate.Id, companyId, normalizedFrom, normalizedTo));
        return exchangeRate;
    }

    /// <summary>
    /// Corrects this rate's Rate and EffectiveDate in place — mirrors
    /// TaxRate.UpdateDetails's "master-data value gets fixed on the same
    /// row, not superseded by a new dated row" pattern, chosen over
    /// versioning because a mistyped rate (e.g. a decimal-point typo) is a
    /// data-entry error worth correcting outright, the same way a wrong
    /// TaxRate.Percentage gets corrected in place rather than left standing
    /// alongside a second, right one. A genuinely new day's rate is a
    /// separate row via Create, not an UpdateRate call on an old one.
    /// FromCurrencyCode/ToCurrencyCode stay immutable after creation — same
    /// "identity fields immutable, value fields correctable" split
    /// BankAccount.UpdateDetails uses for Code/LinkedAccountId.
    /// </summary>
    public void UpdateRate(decimal rate, DateTimeOffset effectiveDate)
    {
        if (rate <= 0)
            throw new ArgumentException("Exchange rate must be greater than zero.", nameof(rate));

        Rate = rate;
        EffectiveDate = effectiveDate;
    }

    public void Deactivate() => IsActive = false;

    private static string NormalizeCurrencyCode(string code, string paramName)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Currency code is required.", paramName);

        var normalized = code.Trim().ToUpperInvariant();
        if (normalized.Length != 3 || !normalized.All(char.IsAsciiLetterUpper))
            throw new ArgumentException("Currency code must be exactly 3 letters (ISO 4217, e.g. \"USD\").", paramName);

        return normalized;
    }
}
