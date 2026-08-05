namespace FusionOS.Modules.Finance.Application.WithholdingTaxTransactions.Contracts;

public sealed record WithholdingTaxTransactionDto(
    Guid Id,
    Guid WithholdingTaxRateId,
    string ReferenceType,
    Guid ReferenceId,
    decimal GrossAmount,
    decimal DeductedAmount,
    DateTimeOffset TransactionDate);
