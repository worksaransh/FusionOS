namespace FusionOS.Modules.Finance.Application.BankAccounts.Contracts;

public sealed record BankAccountDto(
    Guid Id,
    string Code,
    string Name,
    Guid LinkedAccountId,
    string? BankName,
    string? AccountNumberLast4,
    bool IsActive,
    DateTimeOffset CreatedAt);
