namespace FusionOS.Modules.Finance.Application.AccountingPeriods.Contracts;

public sealed record AccountingPeriodDto(Guid Id, DateTimeOffset PeriodStart, DateTimeOffset PeriodEnd, string Status);

/// <summary>Single place that turns an AccountingPeriod aggregate into its DTO, shared by every handler that returns one.</summary>
public static class AccountingPeriodMapper
{
    public static AccountingPeriodDto ToDto(Domain.AccountingPeriods.AccountingPeriod period) => new(
        period.Id, period.PeriodStart, period.PeriodEnd, period.Status.ToString());
}
