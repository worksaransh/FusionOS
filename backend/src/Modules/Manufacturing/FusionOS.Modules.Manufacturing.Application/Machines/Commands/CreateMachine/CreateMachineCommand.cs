using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Manufacturing.Application.Machines.Contracts;

namespace FusionOS.Modules.Manufacturing.Application.Machines.Commands.CreateMachine;

public sealed record CreateMachineCommand(
    Guid CompanyId,
    string Code,
    string Name,
    Guid WorkCenterId)
    : ICommand<MachineDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "manufacturing.machine.create" };
    public string EntityType => nameof(Domain.Machines.Machine);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
