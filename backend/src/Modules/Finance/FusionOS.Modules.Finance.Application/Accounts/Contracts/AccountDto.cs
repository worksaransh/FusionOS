namespace FusionOS.Modules.Finance.Application.Accounts.Contracts;

public sealed record AccountDto(Guid Id, string Code, string Name, string AccountType, Guid? ParentAccountId, bool IsActive, DateTimeOffset CreatedAt);
