using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Manufacturing.Application.Machines.Contracts;

namespace FusionOS.Modules.Manufacturing.Application.Machines.Commands.DeactivateMachine;

public sealed record DeactivateMachineCommand(Guid CompanyId, Guid MachineId)
    : ICommand<MachineDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "manufacturing.machine.deactivate" };
    public string EntityType => nameof(Domain.Machines.Machine);
    public Guid EntityId => MachineId;
    public string Action => "Deactivated";
}
