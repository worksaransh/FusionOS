namespace FusionOS.Modules.Sales.Application.Commissions.Contracts;

public sealed record SalesCommissionRateDto(Guid Id, Guid UserId, decimal RatePercentage);

public sealed record SalesCommissionSummaryLineDto(Guid UserId, decimal TotalInvoicedRevenue, decimal RatePercentage, decimal CommissionAmount);
