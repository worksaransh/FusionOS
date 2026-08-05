using FusionOS.SharedKernel;
using FusionOS.Modules.Manufacturing.Domain.BillOfMaterials.Events;

namespace FusionOS.Modules.Manufacturing.Domain.BillOfMaterials;

/// <summary>
/// Phase 3 — Manufacturing ERP, first slice. A bill of materials defines what a
/// manufactured Product is made of: the parent <see cref="ProductId"/> (an opaque
/// reference into Inventory's Product aggregate — no cross-module FK, same convention
/// as every other product reference in this codebase) plus one or more component
/// <see cref="BomLine"/>s. Deliberately flat, not a multi-level/self-referencing BOM
/// tree — a component that is itself manufactured simply has its own BillOfMaterials;
/// exploding a full multi-level structure is MRP's job, a separate later slice, not
/// this master-data aggregate's.
///
/// Code is the business key (unique per company, checked by the command handler before
/// creation, same split CostCenter/Account use for their Code). A component may not be
/// the parent product itself, and no product may appear twice in the same BOM — both
/// enforced here in the aggregate, not just via validation.
/// </summary>
public sealed class BillOfMaterials : TenantAggregateRoot
{
    private readonly List<BomLine> _lines = new();
    private readonly List<RoutingOperation> _operations = new();

    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public Guid ProductId { get; private set; }
    public bool IsActive { get; private set; } = true;
    public IReadOnlyList<BomLine> Lines => _lines.AsReadOnly();

    /// <summary>
    /// BOM Versioning (Phase 6, 2026-07-22). Starts at 1; <see cref="CreateNewVersion"/>
    /// produces a new row with Version + 1, linked back via <see cref="PreviousVersionId"/>,
    /// and marks this instance's <see cref="IsCurrentVersion"/> false — the old version is
    /// never mutated in place or hard-deleted, same "supersede, don't overwrite" discipline
    /// as Finance's AccountingPeriod close.
    /// </summary>
    public int Version { get; private set; } = 1;

    /// <summary>True only for the newest version in this BOM's version chain — the one Work Orders and MRP should read by default.</summary>
    public bool IsCurrentVersion { get; private set; } = true;

    /// <summary>The BillOfMaterials this one superseded, or null for a version-1 BOM.</summary>
    public Guid? PreviousVersionId { get; private set; }

    /// <summary>
    /// Alternative BOM (Phase 6, 2026-07-22). Multiple active BillOfMaterials rows may
    /// already exist for the same ProductId (nothing enforces one-per-product at the
    /// data-model level — only Code is unique). IsPrimary marks which one a Work Order
    /// should default to when the caller doesn't explicitly pick a BillOfMaterialsId;
    /// the others are "alternatives" (different sourcing/substitute components for the
    /// same finished product). Enforcing "only one primary per product" is the
    /// SetPrimaryBillOfMaterialsCommandHandler's job (it must load every BOM for the
    /// product to clear the others) — this aggregate only tracks its own flag.
    /// </summary>
    public bool IsPrimary { get; private set; } = true;

    /// <summary>The routing's operations in production order — always sorted by SequenceNumber for consumers, regardless of insertion/storage order.</summary>
    public IReadOnlyList<RoutingOperation> Operations => _operations.OrderBy(o => o.SequenceNumber).ToList();

    private BillOfMaterials() { }

    /// <summary>
    /// <paramref name="isPrimary"/> defaults to true so a product's first-ever BOM is
    /// automatically its primary one. When creating an ALTERNATIVE BOM for a product
    /// that already has an active one, the caller (CreateBillOfMaterialsCommandHandler)
    /// must explicitly pass false — this factory has no repository access to check for
    /// existing BOMs itself, so it cannot enforce "only one primary per product" alone.
    /// </summary>
    public static BillOfMaterials Create(Guid companyId, string code, string name, Guid productId, IReadOnlyCollection<BomLineInput> lines, bool isPrimary = true)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Bill of materials code is required.", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Bill of materials name is required.", nameof(name));
        if (productId == Guid.Empty)
            throw new ArgumentException("Manufactured product id is required.", nameof(productId));
        if (lines is null || lines.Count == 0)
            throw new ArgumentException("A bill of materials must have at least one component line.", nameof(lines));

        var bom = new BillOfMaterials
        {
            CompanyId = companyId,
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            ProductId = productId,
            IsPrimary = isPrimary,
        };

        var seen = new HashSet<Guid>();
        foreach (var line in lines)
        {
            if (line.ComponentProductId == productId)
                throw new ArgumentException("A bill of materials component cannot be the manufactured product itself.", nameof(lines));
            if (!seen.Add(line.ComponentProductId))
                throw new ArgumentException($"Component product '{line.ComponentProductId}' appears more than once in the bill of materials.", nameof(lines));

            bom._lines.Add(BomLine.Create(line.ComponentProductId, line.Quantity, line.UnitCost));
        }

        bom.Raise(new BillOfMaterialsCreated(bom.Id, companyId, productId, bom.Code));
        return bom;
    }

    /// <summary>The standard soft-deactivate — a superseded BOM is hidden from active lists, never hard-deleted.</summary>
    public void Deactivate() => IsActive = false;

    /// <summary>
    /// Updates the header-level editable fields only (Name and ProductId) — deliberately
    /// distinct from <see cref="CreateNewVersion"/>: it does not touch Lines/Operations,
    /// does not bump Version, and does not create a new row. Code is excluded — it's the
    /// immutable business key, same convention as UpdateBranchCommand/UpdateCostCenterCommand.
    /// </summary>
    public void UpdateHeader(string name, Guid productId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Bill of materials name is required.", nameof(name));
        if (productId == Guid.Empty)
            throw new ArgumentException("Manufactured product id is required.", nameof(productId));

        Name = name.Trim();
        ProductId = productId;
    }

    /// <summary>Marks this BOM as the product's primary/default one — see the IsPrimary property doc comment for the multi-BOM-per-product coordination this doesn't itself enforce.</summary>
    public void MarkAsPrimary() => IsPrimary = true;

    /// <summary>Clears this BOM's primary flag — called on every other BOM for a product when a new one is promoted to primary.</summary>
    public void ClearPrimary() => IsPrimary = false;

    /// <summary>
    /// BOM Versioning: produces a new BillOfMaterials row for the same product/code at
    /// Version + 1, copying this version's routing operations verbatim (a version bump
    /// is normally a component/quantity change, not a routing rework) while taking a
    /// fresh set of component lines. Marks this instance as no longer current — the
    /// caller must persist both this (now-updated) instance and the returned new one.
    /// </summary>
    public BillOfMaterials CreateNewVersion(string name, IReadOnlyCollection<BomLineInput> lines)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Bill of materials name is required.", nameof(name));
        if (lines is null || lines.Count == 0)
            throw new ArgumentException("A bill of materials must have at least one component line.", nameof(lines));

        var newVersion = new BillOfMaterials
        {
            CompanyId = CompanyId,
            Code = Code,
            Name = name.Trim(),
            ProductId = ProductId,
            Version = Version + 1,
            PreviousVersionId = Id,
            IsCurrentVersion = true,
            IsPrimary = IsPrimary,
        };

        var seen = new HashSet<Guid>();
        foreach (var line in lines)
        {
            if (line.ComponentProductId == ProductId)
                throw new ArgumentException("A bill of materials component cannot be the manufactured product itself.", nameof(lines));
            if (!seen.Add(line.ComponentProductId))
                throw new ArgumentException($"Component product '{line.ComponentProductId}' appears more than once in the bill of materials.", nameof(lines));

            newVersion._lines.Add(BomLine.Create(line.ComponentProductId, line.Quantity, line.UnitCost));
        }

        foreach (var operation in Operations)
            newVersion._operations.Add(RoutingOperation.Create(operation.SequenceNumber, operation.OperationName, operation.WorkCenter, operation.StandardMinutes));

        IsCurrentVersion = false;
        newVersion.Raise(new BillOfMaterialsVersionCreated(newVersion.Id, CompanyId, ProductId, Id, newVersion.Version));
        return newVersion;
    }

    /// <summary>Appends a new routing operation at the end of the sequence (current max SequenceNumber + 1, or 1 if none exist yet).</summary>
    public RoutingOperation AddOperation(string operationName, string workCenter, decimal standardMinutes)
    {
        var nextSequenceNumber = _operations.Count == 0 ? 1 : _operations.Max(o => o.SequenceNumber) + 1;
        var operation = RoutingOperation.Create(nextSequenceNumber, operationName, workCenter, standardMinutes);

        _operations.Add(operation);
        Raise(new RoutingOperationAdded(Id, CompanyId, operation.Id, operation.SequenceNumber, operation.OperationName));
        return operation;
    }

    /// <summary>Removes a routing operation. The remaining operations keep their existing SequenceNumber (gaps are fine — Operations is always exposed in SequenceNumber order).</summary>
    public void RemoveOperation(Guid operationId)
    {
        var operation = _operations.FirstOrDefault(o => o.Id == operationId)
            ?? throw new ArgumentException($"Routing operation '{operationId}' does not exist on this bill of materials.", nameof(operationId));

        _operations.Remove(operation);
        Raise(new RoutingOperationRemoved(Id, CompanyId, operationId));
    }

    /// <summary>
    /// Reassigns every operation's SequenceNumber (1-based) to match the given order.
    /// <paramref name="orderedOperationIds"/> must be a permutation of every operation
    /// id currently on this bill of materials — no partial reorders, no unknown ids.
    /// </summary>
    public void ReorderOperations(IReadOnlyList<Guid> orderedOperationIds)
    {
        if (orderedOperationIds is null || orderedOperationIds.Count != _operations.Count)
            throw new ArgumentException("The reordered list must include every routing operation on this bill of materials exactly once.", nameof(orderedOperationIds));

        var existingIds = _operations.Select(o => o.Id).ToHashSet();
        if (orderedOperationIds.Distinct().Count() != orderedOperationIds.Count || !orderedOperationIds.All(existingIds.Contains))
            throw new ArgumentException("The reordered list must include every routing operation on this bill of materials exactly once.", nameof(orderedOperationIds));

        for (var i = 0; i < orderedOperationIds.Count; i++)
        {
            var operation = _operations.First(o => o.Id == orderedOperationIds[i]);
            operation.Resequence(i + 1);
        }

        Raise(new RoutingOperationsReordered(Id, CompanyId, orderedOperationIds));
    }
}
