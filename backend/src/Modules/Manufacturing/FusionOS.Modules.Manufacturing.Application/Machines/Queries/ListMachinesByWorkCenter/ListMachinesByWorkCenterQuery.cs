using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Manufacturing.Application.Machines.Contracts;

namespace FusionOS.Modules.Manufacturing.Application.Machines.Queries.ListMachinesByWorkCenter;

public sealed record ListMachinesByWorkCenterQuery(Guid CompanyId, Guid WorkCenterId)
    : IQuery<IReadOnlyList<MachineDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "manufacturing.machine.read" };
}
