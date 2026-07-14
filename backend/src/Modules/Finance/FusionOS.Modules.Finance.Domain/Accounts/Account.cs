using FusionOS.SharedKernel;
using FusionOS.Modules.Finance.Domain.Accounts.Events;

namespace FusionOS.Modules.Finance.Domain.Accounts;

/// <summary>
/// Chart of Accounts entry — the GL foundation everything else in Finance
/// (05_MODULE_ROADMAP.md Phase 2) depends on. ParentAccountId is an in-module
/// reference to another Account — same module, so this IS a real FK (unlike
/// Inventory's cross-module ledger references), enabling a standard hierarchical
/// chart (e.g. "1000 Assets" -> "1100 Current Assets" -> "1110 Cash").
/// </summary>
public sealed class Account : TenantAggregateRoot
{
    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public AccountType AccountType { get; private set; }
    public Guid? ParentAccountId { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Account() { }

    public static Account Create(Guid companyId, string code, string name, AccountType accountType, Guid? parentAccountId)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Account code is required.", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Account name is required.", nameof(name));

        var account = new Account
        {
            CompanyId = companyId,
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            AccountType = accountType,
            ParentAccountId = parentAccountId,
        };

        account.Raise(new AccountCreated(account.Id, companyId, account.Code, account.AccountType));
        return account;
    }

    public void Deactivate() => IsActive = false;
}
