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

    /// <summary>The routing's operations in production order — always sorted by SequenceNumber for consumers, regardless of insertion/storage order.</summary>
    public IReadOnlyList<RoutingOperation> Operations => _operations.OrderBy(o => o.SequenceNumber).ToList();

    private BillOfMaterials() { }

    public static BillOfMaterials Create(Guid companyId, string code, string name, Guid productId, IReadOnlyCollection<BomLineInput> lines)
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
        };

        var seen = new HashSet<Guid>();
        foreach (var line in lines)
        {
            if (line.ComponentProductId == productId)
                throw new ArgumentException("A bill of materials component cannot be the manufactured product itself.", nameof(lines));
            if (!seen.Add(line.ComponentProductId))
                throw new ArgumentException($"Component product '{line.ComponentProductId}' appears more than once in the bill of materials.", nameof(lines));

            bom._lines.Add(BomLine.Create(line.ComponentProductId, line.Quantity));
        }

        bom.Raise(new BillOfMaterialsCreated(bom.Id, companyId, productId, bom.Code));
        return bom;
    }

    /// <summary>The standard soft-deactivate — a superseded BOM is hidden from active lists, never hard-deleted.</summary>
    public void Deactivate() => IsActive = false;

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
