namespace FusionOS.Modules.Finance.Application.Settings.Contracts;

public sealed record FinanceSettingsDto(
    Guid CompanyId,
    Guid? DefaultArAccountId,
    Guid? DefaultSalesRevenueAccountId,
    Guid? DefaultApAccountId,
    Guid? DefaultPurchaseExpenseAccountId);
