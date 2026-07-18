using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Contracts;
using FusionOS.Modules.Manufacturing.Domain.BillOfMaterials;

namespace FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Commands.CreateBillOfMaterials;

public sealed record CreateBillOfMaterialsCommand(
    Guid CompanyId,
    string Code,
    string Name,
    Guid ProductId,
    IReadOnlyList<BomLineInput> Lines)
    : ICommand<BillOfMaterialsDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "manufacturing.bill-of-materials.create" };
    public string EntityType => nameof(Domain.BillOfMaterials.BillOfMaterials);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
