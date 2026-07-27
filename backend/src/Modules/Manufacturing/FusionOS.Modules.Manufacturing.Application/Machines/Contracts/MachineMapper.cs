namespace FusionOS.Modules.Manufacturing.Application.Machines.Contracts;

/// <summary>Single place that turns a Machine aggregate into its DTO, shared by every handler that returns one.</summary>
public static class MachineMapper
{
    public static MachineDto ToDto(Domain.Machines.Machine machine) => new(
        machine.Id,
        machine.Code,
        machine.Name,
        machine.WorkCenterId,
        machine.IsActive);
}
