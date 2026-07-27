namespace FusionOS.Modules.Manufacturing.Application.WorkCenters.Contracts;

public sealed record WorkCenterDto(Guid Id, string Code, string Name, decimal CapacityHoursPerDay, bool IsActive);
