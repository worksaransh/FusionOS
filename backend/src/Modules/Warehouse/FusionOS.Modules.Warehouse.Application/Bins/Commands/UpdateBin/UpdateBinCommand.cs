using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Warehouse.Application.Bins.Contracts;

namespace FusionOS.Modules.Warehouse.Application.Bins.Commands.UpdateBin;

/// <summary>ZoneId and Code are intentionally not editable here — see Bin.UpdateDetails.</summary>
public sealed record UpdateBinCommand(Guid CompanyId, Guid Id, string Name)
    : ICommand<BinDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "warehouse.bin.update" };
    public string EntityType => nameof(Domain.Bins.Bin);
    public Guid EntityId => Id;
    public string Action => "Updated";
}
