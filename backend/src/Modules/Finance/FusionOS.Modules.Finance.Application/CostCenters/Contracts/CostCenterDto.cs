namespace FusionOS.Modules.Finance.Application.CostCenters.Contracts;

public sealed record CostCenterDto(Guid Id, string Code, string Name, bool IsActive, DateTimeOffset CreatedAt);
