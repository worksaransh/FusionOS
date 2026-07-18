using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Maintenance.Application.Assets.Contracts;

namespace FusionOS.Modules.Maintenance.Application.Assets.Commands.CreateAsset;

public sealed record CreateAssetCommand(Guid CompanyId, string Code, string Name, string? Location)
    : ICommand<AssetDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "maintenance.asset.create" };
    public string EntityType => nameof(Domain.Assets.Asset);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
