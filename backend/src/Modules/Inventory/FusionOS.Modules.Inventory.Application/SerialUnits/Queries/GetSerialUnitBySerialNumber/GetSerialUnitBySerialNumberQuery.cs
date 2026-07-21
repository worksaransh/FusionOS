using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Inventory.Application.SerialUnits.Contracts;

namespace FusionOS.Modules.Inventory.Application.SerialUnits.Queries.GetSerialUnitBySerialNumber;

/// <summary>The real "scan a serial and find it" use case — an exact, company-wide lookup by serial number (not scoped to a single product, since the scanner doesn't know the product up front).</summary>
public sealed record GetSerialUnitBySerialNumberQuery(Guid CompanyId, string SerialNumber)
    : IQuery<SerialUnitDto?>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "inventory.serial.read" };
}
