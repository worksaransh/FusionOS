namespace FusionOS.Modules.Crm.Application.Accounts.Contracts;

public sealed record AccountDto(
    Guid Id,
    string Name,
    string? Industry,
    string? Website,
    bool IsActive,
    DateTimeOffset CreatedAt);

/// <summary>Single place that turns an Account aggregate into its DTO, shared by every handler that returns one.</summary>
public static class AccountMapper
{
    public static AccountDto ToDto(Domain.Accounts.Account account) => new(
        account.Id,
        account.Name,
        account.Industry,
        account.Website,
        account.IsActive,
        account.CreatedAt);
}
