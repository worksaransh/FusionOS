namespace FusionOS.Modules.Crm.Application.Accounts.Contracts;

public sealed record AccountDto(
    Guid Id,
    string Name,
    string? Industry,
    string? Website,
    bool IsActive,
    DateTimeOffset CreatedAt,
    Guid? OwnerId,
    bool IsPossibleDuplicate = false);

/// <summary>Single place that turns an Account aggregate into its DTO, shared by every handler that returns one.</summary>
public static class AccountMapper
{
    /// <summary>Plain mapping with no duplicate-warning context — used by every read path (List/GetById) where the concept doesn't apply.</summary>
    public static AccountDto ToDto(Domain.Accounts.Account account) => ToDto(account, isPossibleDuplicate: false);

    /// <summary>Used by CreateAccountCommandHandler to attach the advisory IsPossibleDuplicate flag it computed before creating.</summary>
    public static AccountDto ToDto(Domain.Accounts.Account account, bool isPossibleDuplicate) => new(
        account.Id,
        account.Name,
        account.Industry,
        account.Website,
        account.IsActive,
        account.CreatedAt,
        account.OwnerId,
        isPossibleDuplicate);
}
