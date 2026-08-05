using FusionOS.SharedKernel;
using FusionOS.Modules.Maintenance.Domain.ChecklistTemplates.Events;

namespace FusionOS.Modules.Maintenance.Domain.ChecklistTemplates;

/// <summary>
/// Maintenance Checklists/Procedures (2026-07-23 follow-up): a reusable, named
/// set of procedure steps ("Quarterly generator PM") a technician can be
/// handed when a MaintenanceRequest is created against it. Deliberately a
/// standalone master-data aggregate, not owned by Asset or MaintenanceSchedule
/// — the same procedure ("Lockout/tagout, drain oil, replace filter...") is
/// commonly reused across many different machines, same "shared master data,
/// referenced by id" convention as Manufacturing's BillOfMaterials being
/// referenced by WorkOrder rather than owned by it.
///
/// Owns its ChecklistItems as an EF-owned collection (HasField("_items"),
/// same convention as BillOfMaterials._lines/_operations) — a step has no
/// independent lifecycle outside its template.
/// </summary>
public sealed class ChecklistTemplate : TenantAggregateRoot
{
    private readonly List<ChecklistItem> _items = new();

    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;
    public IReadOnlyList<ChecklistItem> Items => _items.OrderBy(i => i.SequenceNumber).ToList();

    private ChecklistTemplate() { }

    public static ChecklistTemplate Create(Guid companyId, string name, string? description, IReadOnlyCollection<string> stepDescriptions)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Checklist template name is required.", nameof(name));
        if (stepDescriptions is null || stepDescriptions.Count == 0)
            throw new ArgumentException("A checklist template must have at least one step.", nameof(stepDescriptions));

        var template = new ChecklistTemplate
        {
            CompanyId = companyId,
            Name = name.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
        };

        var sequence = 1;
        foreach (var step in stepDescriptions)
            template._items.Add(ChecklistItem.Create(sequence++, step));

        template.Raise(new ChecklistTemplateCreated(template.Id, companyId, template.Name));
        return template;
    }

    /// <summary>Soft-deactivate — a retired procedure is hidden from active lists, never hard-deleted. Same convention as Asset.Deactivate.</summary>
    public void Deactivate() => IsActive = false;

    /// <summary>
    /// Update replaces the whole step list, same "no independent lifecycle outside
    /// its template" reasoning as Create — steps are re-sequenced from scratch
    /// rather than diffed against the previous set.
    /// </summary>
    public void UpdateDetails(string name, string? description, IReadOnlyCollection<string> stepDescriptions)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Checklist template name is required.", nameof(name));
        if (stepDescriptions is null || stepDescriptions.Count == 0)
            throw new ArgumentException("A checklist template must have at least one step.", nameof(stepDescriptions));

        Name = name.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();

        _items.Clear();
        var sequence = 1;
        foreach (var step in stepDescriptions)
            _items.Add(ChecklistItem.Create(sequence++, step));
    }
}
