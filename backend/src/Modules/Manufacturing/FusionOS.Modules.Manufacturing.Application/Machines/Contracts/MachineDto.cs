namespace FusionOS.Modules.Manufacturing.Application.Machines.Contracts;

public sealed record MachineDto(Guid Id, string Code, string Name, Guid WorkCenterId, bool IsActive);
