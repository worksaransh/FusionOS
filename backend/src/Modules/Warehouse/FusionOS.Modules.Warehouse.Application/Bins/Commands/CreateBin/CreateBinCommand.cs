using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Warehouse.Application.Bins.Contracts;

namespace FusionOS.Modules.Warehouse.Application.Bins.Commands.CreateBin;

public sealed record CreateBinCommand(Guid CompanyId, Guid ZoneId, string Name, string Code)
    : ICommand<BinDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "warehouse.bin.create" };
    public string EntityType => nameof(Domain.Bins.Bin);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
