using FusionOS.SharedKernel;
using FusionOS.Modules.Finance.Domain.BankAccounts.Events;

namespace FusionOS.Modules.Finance.Domain.BankAccounts;

/// <summary>
/// M8d — Finance depth: bank reconciliation, the fourth of the Phase M8 a–h
/// sub-slices. Master data for a bank account FusionOS's company holds,
/// mirroring CostCenter.cs's simplicity (Code/Name/IsActive) plus a
/// mandatory <see cref="LinkedAccountId"/> — the GL Account (Chart of
/// Accounts, same module) this bank account reconciles against, a real
/// in-module FK the same way TaxRate.TaxJurisdictionId is. This factory does
/// not verify LinkedAccountId actually exists in Finance's Account table —
/// same "domain enforces shape, handler enforces cross-aggregate existence"
/// split CreateTaxRateCommandHandler uses for TaxJurisdictionId (via
/// ITaxRateRepository.TaxJurisdictionExistsAsync); here it's
/// CreateBankAccountCommandHandler checking IAccountRepository.ExistsAsync.
///
/// <see cref="AccountNumberLast4"/> deliberately stores at most the last 4
/// digits of a bank account number, never the full number — a deliberate
/// security-conscious design choice (07_SECURITY.md), the same spirit as
/// never storing a full card PAN. Both BankName and AccountNumberLast4 are
/// optional free text purely so a human can recognize which real-world
/// account this is; neither is validated against any external bank registry
/// or checksum.
/// </summary>
public sealed class BankAccount : TenantAggregateRoot
{
    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public Guid LinkedAccountId { get; private set; }
    public string? BankName { get; private set; }
    public string? AccountNumberLast4 { get; private set; }
    public bool IsActive { get; private set; } = true;

    private BankAccount() { }

    public static BankAccount Create(Guid companyId, string code, string name, Guid linkedAccountId, string? bankName, string? accountNumberLast4)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Bank account code is required.", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Bank account name is required.", nameof(name));
        if (linkedAccountId == Guid.Empty)
            throw new ArgumentException("A linked GL account is required.", nameof(linkedAccountId));
        if (!string.IsNullOrWhiteSpace(accountNumberLast4) && accountNumberLast4.Trim().Length > 4)
            throw new ArgumentException("Only the last 4 digits of an account number may be stored — never the full number.", nameof(accountNumberLast4));

        var bankAccount = new BankAccount
        {
            CompanyId = companyId,
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            LinkedAccountId = linkedAccountId,
            BankName = string.IsNullOrWhiteSpace(bankName) ? null : bankName.Trim(),
            AccountNumberLast4 = string.IsNullOrWhiteSpace(accountNumberLast4) ? null : accountNumberLast4.Trim(),
        };

        bankAccount.Raise(new BankAccountCreated(bankAccount.Id, companyId, bankAccount.Code));
        return bankAccount;
    }

    /// <summary>
    /// Updates the mutable master-data fields. Code, CompanyId, and
    /// LinkedAccountId all stay immutable after creation — Code is the
    /// tenant-scoped business key (same convention as CostCenter/Account),
    /// and LinkedAccountId is a structural link a user shouldn't repoint onto
    /// a different GL account once reconciliation history has started
    /// accumulating against it.
    /// </summary>
    public void UpdateDetails(string name, string? bankName, string? accountNumberLast4)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Bank account name is required.", nameof(name));
        if (!string.IsNullOrWhiteSpace(accountNumberLast4) && accountNumberLast4.Trim().Length > 4)
            throw new ArgumentException("Only the last 4 digits of an account number may be stored — never the full number.", nameof(accountNumberLast4));

        Name = name.Trim();
        BankName = string.IsNullOrWhiteSpace(bankName) ? null : bankName.Trim();
        AccountNumberLast4 = string.IsNullOrWhiteSpace(accountNumberLast4) ? null : accountNumberLast4.Trim();
    }

    public void Deactivate() => IsActive = false;
}
