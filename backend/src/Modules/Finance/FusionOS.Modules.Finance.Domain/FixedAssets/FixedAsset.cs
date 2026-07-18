using FusionOS.SharedKernel;
using FusionOS.Modules.Finance.Domain.FixedAssets.Events;

namespace FusionOS.Modules.Finance.Domain.FixedAssets;

/// <summary>
/// M8g — Finance depth: fixed assets, the seventh of the Phase M8 a–h
/// sub-slices. Master data for a company-owned depreciable asset (e.g. "Delivery
/// Van #3") plus enough of its own fields (AcquisitionCost/SalvageValue/
/// UsefulLifeMonths) to support a pure, on-demand straight-line depreciation
/// calculation — see GetDepreciationScheduleQueryHandler's own doc comment for
/// that calculation and why nothing about it is persisted here or anywhere
/// else.
///
/// <b>Scope decision (Phase M8g, 2026-07-17), documented here the same way
/// every other M8 sub-slice's class doc comment documents its scope-out:</b>
/// this slice is master data plus a read-only calculation only. There is no
/// automated monthly depreciation run that posts real JournalEntries to the
/// GL, no disposal gain/loss calculation (that needs sale proceeds, which
/// this aggregate never collects) posted to the GL, and no depreciation
/// method other than straight-line (no declining-balance, no
/// units-of-production). Full fixed-asset accounting — a depreciation-run
/// process, disposal gain/loss posting, and additional depreciation methods —
/// is a distinct, separately-scoped, materially larger future phase.
///
/// AssetAccountId and AccumulatedDepreciationAccountId are both in-module FKs
/// into Account, validated by the command handler before this aggregate is
/// created — same handler-checks-existence split BudgetLine's Create uses
/// for AccountId (see CreateFixedAssetCommandHandler). CostCenterId is
/// likewise optional and, if present, validated the same way BudgetLine
/// validates its own optional CostCenterId.
///
/// AcquisitionDate is <see cref="DateTimeOffset"/>, matching the date type
/// Budget.PeriodStart/JournalEntry.EntryDate/ExchangeRate.EffectiveDate
/// already use — not DateOnly, which nothing in this codebase uses.
/// </summary>
public sealed class FixedAsset : TenantAggregateRoot
{
    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public Guid AssetAccountId { get; private set; }

    /// <summary>
    /// Nullable — not every company tracks accumulated depreciation in a
    /// dedicated GL account at the moment an asset is first registered (e.g.
    /// a small company may set that account up later, or route all
    /// depreciation through one shared control account decided separately
    /// from asset registration). Since this slice never posts depreciation
    /// to the GL itself (see class doc comment), there is no requirement that
    /// this be populated up front — it is captured here only so a future
    /// depreciation-posting slice has somewhere to point to without needing
    /// a schema change.
    /// </summary>
    public Guid? AccumulatedDepreciationAccountId { get; private set; }

    public Guid? CostCenterId { get; private set; }
    public DateTimeOffset AcquisitionDate { get; private set; }
    public decimal AcquisitionCost { get; private set; }
    public decimal SalvageValue { get; private set; }
    public int UsefulLifeMonths { get; private set; }
    public bool IsDisposed { get; private set; }
    public DateTimeOffset? DisposedDate { get; private set; }
    public bool IsActive { get; private set; } = true;

    private FixedAsset() { }

    public static FixedAsset Create(
        Guid companyId,
        string code,
        string name,
        Guid assetAccountId,
        Guid? accumulatedDepreciationAccountId,
        Guid? costCenterId,
        DateTimeOffset acquisitionDate,
        decimal acquisitionCost,
        decimal salvageValue,
        int usefulLifeMonths)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Fixed asset code is required.", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Fixed asset name is required.", nameof(name));
        if (assetAccountId == Guid.Empty)
            throw new ArgumentException("Asset account id is required.", nameof(assetAccountId));
        if (acquisitionCost <= 0)
            throw new ArgumentException("Acquisition cost must be greater than zero.", nameof(acquisitionCost));
        if (salvageValue < 0)
            throw new ArgumentException("Salvage value cannot be negative.", nameof(salvageValue));
        if (salvageValue >= acquisitionCost)
            throw new ArgumentException("Salvage value must be less than acquisition cost — a salvage value equal to or greater than the original cost would make depreciation nonsensical.", nameof(salvageValue));
        if (usefulLifeMonths <= 0)
            throw new ArgumentException("Useful life (months) must be greater than zero.", nameof(usefulLifeMonths));

        var fixedAsset = new FixedAsset
        {
            CompanyId = companyId,
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            AssetAccountId = assetAccountId,
            AccumulatedDepreciationAccountId = accumulatedDepreciationAccountId,
            CostCenterId = costCenterId,
            AcquisitionDate = acquisitionDate,
            AcquisitionCost = acquisitionCost,
            SalvageValue = salvageValue,
            UsefulLifeMonths = usefulLifeMonths,
            IsDisposed = false,
        };

        fixedAsset.Raise(new FixedAssetCreated(fixedAsset.Id, companyId, fixedAsset.Code));
        return fixedAsset;
    }

    /// <summary>
    /// Updates only the non-financial master-data fields. AcquisitionCost,
    /// SalvageValue, and UsefulLifeMonths are deliberately NOT editable after
    /// creation — the same "business key/financial fact, not a casual edit"
    /// reasoning CostCenter.Code and Account.Code use for their own
    /// immutable-after-create fields, but for a different underlying reason:
    /// changing an asset's cost/salvage/life after the fact would silently
    /// invalidate any depreciation figure already calculated or reported to
    /// someone from GetDepreciationScheduleQuery, with no record that the
    /// inputs ever changed. If a genuine correction is needed (e.g. the
    /// acquisition cost was mis-entered), that is an out-of-band data-fix —
    /// a deliberate, auditable exception handled outside the normal Update
    /// command — not a routine edit path this aggregate exposes.
    /// AssetAccountId/AccumulatedDepreciationAccountId are also not editable
    /// here for the same reason CostCenterId is not part of BudgetLine's
    /// UpdateAmount: they are this asset's identity-adjacent references, not
    /// name/cost-center classification.
    /// </summary>
    public void UpdateDetails(string name, Guid? costCenterId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Fixed asset name is required.", nameof(name));

        Name = name.Trim();
        CostCenterId = costCenterId;
    }

    /// <summary>
    /// Marks the asset disposed — a genuine, one-way business-meaningful
    /// state change, distinct from <see cref="Deactivate"/> (see that
    /// method's own doc comment for the distinction). Deliberately does NOT
    /// calculate or post any gain/loss-on-disposal amount: that requires
    /// knowing the actual sale/scrap proceeds (which this command never
    /// collects) and posting the result to the GL, both explicitly out of
    /// scope for this slice (see class doc comment). A future disposal
    /// accounting slice can add that on top of this DisposedDate flag without
    /// needing to touch this method's shape.
    /// </summary>
    public void Dispose(DateTimeOffset disposedDate)
    {
        if (IsDisposed)
            throw new InvalidOperationException("This fixed asset has already been disposed.");
        if (disposedDate < AcquisitionDate)
            throw new ArgumentException("Disposed date cannot be before the acquisition date.", nameof(disposedDate));

        IsDisposed = true;
        DisposedDate = disposedDate;
    }

    /// <summary>
    /// The standard "hide from active lists" soft-deactivate every other
    /// aggregate in this codebase exposes — separate from <see cref="Dispose"/>,
    /// which records a real-world event (the asset was sold/scrapped/retired).
    /// The two are independent and can coexist: a disposed asset is normally
    /// also deactivated (nothing further should happen to it), but an asset
    /// can equally be deactivated without ever being disposed (e.g. a
    /// data-entry mistake being hidden, not a real asset being retired), and
    /// a disposed asset is not force-deactivated automatically by Dispose
    /// itself — the caller decides both independently.
    /// </summary>
    public void Deactivate() => IsActive = false;
}
