using FusionOS.SharedKernel;

namespace FusionOS.Modules.Crm.Domain.Tags;

/// <summary>
/// Phase 4 — CRM, segmentation/tagging slice. A tenant-scoped label (e.g. "VIP",
/// "Cold", "Enterprise") that can be attached to any CRM entity via
/// <see cref="EntityTag"/>. Name uniqueness per company is enforced by the handler
/// via ITagRepository (existence/duplicate check), not here — same division of
/// responsibility as CRM's other create commands. Colour is a free-form optional
/// hex/name string purely for UI badge rendering; the domain does not validate its
/// format.
/// </summary>
public sealed class Tag : TenantAggregateRoot
{
    public string Name { get; private set; } = default!;
    public string? Color { get; private set; }

    private Tag() { }

    public static Tag Create(Guid companyId, string name, string? color)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Tag name is required.", nameof(name));

        return new Tag
        {
            CompanyId = companyId,
            Name = name.Trim(),
            Color = string.IsNullOrWhiteSpace(color) ? null : color.Trim(),
        };
    }
}
