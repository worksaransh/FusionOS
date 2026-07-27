namespace FusionOS.Modules.Maintenance.Domain.ChecklistTemplates;

/// <summary>
/// One procedure step of a <see cref="ChecklistTemplate"/>: free-text
/// Description plus its position in the procedure. Same documented,
/// reviewed exception to the "every table has audit/tenant columns" rule
/// (04_DATABASE_GUIDELINES.md §3) as Manufacturing's BomLine/RoutingOperation
/// — a step's lifecycle is owned entirely by its parent ChecklistTemplate.
/// </summary>
public sealed class ChecklistItem
{
    public Guid Id { get; private set; }
    public string Description { get; private set; } = default!;
    public int SequenceNumber { get; private set; }

    private ChecklistItem() { }

    internal static ChecklistItem Create(int sequenceNumber, string description)
    {
        if (sequenceNumber <= 0)
            throw new ArgumentException("Sequence number must be greater than zero.", nameof(sequenceNumber));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Checklist item description is required.", nameof(description));

        return new ChecklistItem
        {
            Id = Guid.NewGuid(),
            SequenceNumber = sequenceNumber,
            Description = description.Trim(),
        };
    }
}
