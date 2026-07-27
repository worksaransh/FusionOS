namespace FusionOS.Modules.Maintenance.Domain.MaintenanceRequests;

/// <summary>
/// One checklist step snapshotted onto a <see cref="MaintenanceRequest"/> at creation time
/// from a ChecklistTemplates.ChecklistTemplate, when the caller opts into one via
/// CreateMaintenanceRequestCommand's ChecklistTemplateId. Snapshotted (Description copied by
/// value) rather than referencing the template's ChecklistItem by id so a later edit to the
/// template never retroactively changes an in-flight request's checklist — same "capture the
/// value at the point of the transaction" discipline as Manufacturing's WorkOrderComponent
/// snapshotting BomLine.UnitCost. Same documented no-audit/tenant-columns exception as every
/// other line/child entity owned by a parent aggregate.
/// </summary>
public sealed class MaintenanceRequestChecklistItem
{
    public Guid Id { get; private set; }
    public string Description { get; private set; } = default!;
    public bool IsCompleted { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }

    private MaintenanceRequestChecklistItem() { }

    internal static MaintenanceRequestChecklistItem Create(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Checklist item description is required.", nameof(description));

        return new MaintenanceRequestChecklistItem
        {
            Id = Guid.NewGuid(),
            Description = description.Trim(),
        };
    }

    /// <summary>Marks this step done. Idempotent-safe at the caller's discretion — this itself does not reject an already-completed item, same "no error on a no-op re-completion" restraint as some other terminal-state setters in this codebase; callers that need strictness can check IsCompleted first.</summary>
    internal void Complete()
    {
        IsCompleted = true;
        CompletedAt = DateTimeOffset.UtcNow;
    }
}
