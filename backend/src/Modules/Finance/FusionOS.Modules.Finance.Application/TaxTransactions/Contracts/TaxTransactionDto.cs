namespace FusionOS.Modules.Finance.Application.TaxTransactions.Contracts;

public sealed record TaxTransactionDto(
    Guid Id,
    string Direction,
    Guid TaxRateId,
    decimal TaxAmount,
    DateTimeOffset TransactionDate,
    string SourceEntityType,
    Guid SourceEntityId);
