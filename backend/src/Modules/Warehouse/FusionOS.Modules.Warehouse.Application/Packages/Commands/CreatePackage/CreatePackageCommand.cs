using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Warehouse.Application.Packages.Contracts;
using FusionOS.Modules.Warehouse.Domain.Packages;

namespace FusionOS.Modules.Warehouse.Application.Packages.Commands.CreatePackage;

/// <summary>
/// PickListId must reference an existing pick list belonging to this company whose status is
/// already Packed — validated by the handler (same-module reference, like BinId on
/// CreatePickListCommand). ProductId per line is an opaque cross-module reference into Inventory,
/// not validated here (see Package.cs's doc comment).
/// </summary>
public sealed record CreatePackageCommand(
    Guid CompanyId,
    Guid PickListId,
    string PackageNumber,
    decimal? WeightKg,
    decimal? LengthCm,
    decimal? WidthCm,
    decimal? HeightCm,
    IReadOnlyList<PackageLineInput> Lines)
    : ICommand<PackageDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "warehouse.package.create" };
    public string EntityType => nameof(Domain.Packages.Package);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
