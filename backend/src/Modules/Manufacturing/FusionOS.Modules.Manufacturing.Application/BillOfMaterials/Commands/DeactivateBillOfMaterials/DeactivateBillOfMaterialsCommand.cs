using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Contracts;

namespace FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Commands.DeactivateBillOfMaterials;

public sealed record DeactivateBillOfMaterialsCommand(Guid CompanyId, Guid BillOfMaterialsId)
    : ICommand<BillOfMaterialsDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "manufacturing.bill-of-materials.deactivate" };
    public string EntityType => nameof(Domain.BillOfMaterials.BillOfMaterials);
    public Guid EntityId => BillOfMaterialsId;
    public string Action => "Deactivated";
}
