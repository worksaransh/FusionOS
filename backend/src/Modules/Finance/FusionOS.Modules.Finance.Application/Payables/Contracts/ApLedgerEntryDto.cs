namespace FusionOS.Modules.Finance.Application.Payables.Contracts;

public sealed record ApLedgerEntryDto(Guid Id, Guid SupplierId, Guid? PurchaseOrderId, decimal Amount, string Description, DateTimeOffset TransactionDate, Guid? BankAccountId = null);

public sealed record SupplierBalanceDto(Guid SupplierId, decimal Balance);
