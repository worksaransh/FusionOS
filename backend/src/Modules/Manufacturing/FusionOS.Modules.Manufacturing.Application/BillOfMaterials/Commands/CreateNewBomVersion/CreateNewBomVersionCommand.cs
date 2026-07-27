using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Contracts;
using FusionOS.Modules.Manufacturing.Domain.BillOfMaterials;

namespace FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Commands.CreateNewBomVersion;

/// <summary>
/// BOM Versioning (Phase 6, 2026-07-22). Supersedes <see cref="BillOfMaterialsId"/> with a
/// new Version + 1 row — the handler persists BOTH the (now-mutated) original and the new
/// version, same "supersede, don't overwrite" shape as CreateNewVersion's own doc comment.
/// </summary>
public sealed record CreateNewBomVersionCommand(
    Guid CompanyId,
    Guid BillOfMaterialsId,
    string Name,
    IReadOnlyList<BomLineInput> Lines)
    : ICommand<BillOfMaterialsDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "manufacturing.bill-of-materials.version" };
    public string EntityType => nameof(Domain.BillOfMaterials.BillOfMaterials);
    public Guid EntityId => BillOfMaterialsId;
    public string Action => "VersionCreated";
}
