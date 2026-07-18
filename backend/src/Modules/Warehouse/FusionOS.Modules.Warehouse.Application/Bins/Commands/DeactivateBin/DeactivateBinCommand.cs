using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Warehouse.Application.Bins.Contracts;

namespace FusionOS.Modules.Warehouse.Application.Bins.Commands.DeactivateBin;

/// <summary>Soft-deactivation only — never deletes the row (08_API_STANDARDS.md / 04_DATABASE_GUIDELINES.md).</summary>
public sealed record DeactivateBinCommand(Guid CompanyId, Guid Id)
    : ICommand<BinDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "warehouse.bin.deactivate" };
    public string EntityType => nameof(Domain.Bins.Bin);
    public Guid EntityId => Id;
    public string Action => "Deactivated";
}
