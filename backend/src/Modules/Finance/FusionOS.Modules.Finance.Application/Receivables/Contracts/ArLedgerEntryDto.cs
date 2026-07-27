namespace FusionOS.Modules.Finance.Application.Receivables.Contracts;

public sealed record ArLedgerEntryDto(Guid Id, Guid CustomerId, Guid InvoiceId, decimal Amount, string Description, DateTimeOffset TransactionDate, Guid? BankAccountId = null);

public sealed record CustomerBalanceDto(Guid CustomerId, decimal Balance);
