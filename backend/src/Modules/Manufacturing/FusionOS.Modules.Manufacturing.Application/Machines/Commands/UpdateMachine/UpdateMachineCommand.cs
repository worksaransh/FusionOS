using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Manufacturing.Application.Machines.Contracts;

namespace FusionOS.Modules.Manufacturing.Application.Machines.Commands.UpdateMachine;

/// <summary>Update deliberately excludes Code — it's the immutable business key, same convention as UpdateBranchCommand/UpdateCostCenterCommand. WorkCenterId may be reassigned; the handler must verify the new work center exists, same as CreateMachineCommandHandler.</summary>
public sealed record UpdateMachineCommand(Guid CompanyId, Guid MachineId, string Name, Guid WorkCenterId)
    : ICommand<MachineDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "manufacturing.machine.update" };
    public string EntityType => nameof(Domain.Machines.Machine);
    public Guid EntityId => MachineId;
    public string Action => "Updated";
}
