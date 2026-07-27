using FusionOS.SharedKernel;
using FusionOS.Modules.Maintenance.Domain.Assets.Events;

namespace FusionOS.Modules.Maintenance.Domain.Assets;

/// <summary>
/// Phase 5 — Maintenance, first slice: the machine register
/// (05_MODULE_ROADMAP.md's "Machine register" line item). Pure master data
/// (Code/Name/Location/IsActive), same shape as Finance's CostCenter — no
/// hierarchy, no meter-reading tracking yet; that is a separately scoped
/// follow-up once a MaintenanceRequest exists to reference this.
/// Location is a plain optional string, not a cross-module WarehouseId
/// reference — a machine's physical location description ("Line 2, Bay 4")
/// doesn't need to resolve to a real Warehouse aggregate for this slice to
/// be useful, and adding that reference now would be scope not asked for.
///
/// Warranty tracking (2026-07-23 follow-up) added WarrantyExpirationDate/
/// WarrantyProvider — both nullable/optional since most Assets in this
/// register (older machines, internally built equipment) won't have an
/// active manufacturer warranty. WarrantyProvider is plain free text (same
/// "no dedicated aggregate" convention as RoutingOperation.WorkCenter in
/// Manufacturing) — a Supplier/Vendor master-data link is not needed for
/// this slice. Since Asset previously only had Create/Deactivate, this pass
/// also added UpdateAssetDetails — the first general-purpose editor for
/// Name/Location/warranty fields (Code remains immutable after creation,
/// same "the business key isn't editable" convention as CostCenter/BOM).
/// </summary>
public sealed class Asset : TenantAggregateRoot
{
    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public string? Location { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset? WarrantyExpirationDate { get; private set; }
    public string? WarrantyProvider { get; private set; }

    private Asset() { }

    public static Asset Create(Guid companyId, string code, string name, string? location)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Asset code is required.", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Asset name is required.", nameof(name));

        var asset = new Asset
        {
            CompanyId = companyId,
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Location = string.IsNullOrWhiteSpace(location) ? null : location.Trim(),
        };

        asset.Raise(new AssetCreated(asset.Id, companyId, asset.Code));
        return asset;
    }

    public void Deactivate() => IsActive = false;

    /// <summary>
    /// Updates the mutable master-data fields. Code is not editable — same "the
    /// business key is set at creation" convention as MaintenanceSchedule.UpdateDetails
    /// excluding AssetId. WarrantyExpirationDate/WarrantyProvider are both optional —
    /// passing null for either simply clears that field (e.g. once a warranty lapses).
    /// </summary>
    public void UpdateAssetDetails(string name, string? location, DateTimeOffset? warrantyExpirationDate, string? warrantyProvider)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Asset name is required.", nameof(name));

        Name = name.Trim();
        Location = string.IsNullOrWhiteSpace(location) ? null : location.Trim();
        WarrantyExpirationDate = warrantyExpirationDate;
        WarrantyProvider = string.IsNullOrWhiteSpace(warrantyProvider) ? null : warrantyProvider.Trim();
    }
}
