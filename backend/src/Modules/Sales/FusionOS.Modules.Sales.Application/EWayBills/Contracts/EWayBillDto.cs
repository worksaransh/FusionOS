namespace FusionOS.Modules.Sales.Application.EWayBills.Contracts;

public sealed record EWayBillDto(Guid Id, Guid DispatchId, string EwbNumber, DateTimeOffset GeneratedAt, DateTimeOffset ValidUntil);
