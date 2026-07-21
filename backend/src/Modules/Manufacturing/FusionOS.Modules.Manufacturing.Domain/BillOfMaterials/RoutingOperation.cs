namespace FusionOS.Modules.Manufacturing.Domain.BillOfMaterials;

/// <summary>
/// One step in a <see cref="BillOfMaterials"/>' production routing: the ordered
/// sequence of operations (e.g. "Cut" -> "Assemble" -> "Paint") a manufactured
/// product moves through, plus a free-text work center/machine reference — the
/// same documented "plain text, no dedicated aggregate" convention Quality's own
/// inspection references use, since a Machine/WorkCenter master-data aggregate is
/// not yet a real need for this first slice. Same documented no-audit/tenant-columns
/// exception as BomLine/WorkOrderComponent: a routing operation's lifecycle is owned
/// entirely by its parent BillOfMaterials.
///
/// SequenceNumber is assigned by the parent aggregate (append = current max + 1) and
/// only ever changed via <see cref="Resequence"/>, which the aggregate calls when
/// reordering the whole routing — never set directly by a caller.
/// </summary>
public sealed class RoutingOperation
{
    public Guid Id { get; private set; }
    public int SequenceNumber { get; private set; }
    public string OperationName { get; private set; } = default!;
    public string WorkCenter { get; private set; } = default!;
    public decimal StandardMinutes { get; private set; }

    private RoutingOperation() { }

    internal static RoutingOperation Create(int sequenceNumber, string operationName, string workCenter, decimal standardMinutes)
    {
        if (sequenceNumber <= 0)
            throw new ArgumentException("Sequence number must be greater than zero.", nameof(sequenceNumber));
        if (string.IsNullOrWhiteSpace(operationName))
            throw new ArgumentException("Operation name is required.", nameof(operationName));
        if (string.IsNullOrWhiteSpace(workCenter))
            throw new ArgumentException("Work center is required.", nameof(workCenter));
        if (standardMinutes < 0)
            throw new ArgumentException("Standard minutes cannot be negative.", nameof(standardMinutes));

        return new RoutingOperation
        {
            Id = Guid.NewGuid(),
            SequenceNumber = sequenceNumber,
            OperationName = operationName.Trim(),
            WorkCenter = workCenter.Trim(),
            StandardMinutes = standardMinutes,
        };
    }

    /// <summary>Reassigns this operation's position in the routing — only called by the parent aggregate's ReorderOperations.</summary>
    internal void Resequence(int sequenceNumber)
    {
        if (sequenceNumber <= 0)
            throw new ArgumentException("Sequence number must be greater than zero.", nameof(sequenceNumber));

        SequenceNumber = sequenceNumber;
    }
}
