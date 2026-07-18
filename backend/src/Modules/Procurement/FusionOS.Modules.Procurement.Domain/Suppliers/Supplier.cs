using FusionOS.SharedKernel;
using FusionOS.Modules.Procurement.Domain.Suppliers.Events;

namespace FusionOS.Modules.Procurement.Domain.Suppliers;

/// <summary>
/// The anchor aggregate for Procurement (05_MODULE_ROADMAP.md Phase 1). RFQ,
/// supplier comparison, Purchase Orders, approvals, and goods receipt all
/// reference a Supplier — this is the narrow first cut: supplier master data.
/// </summary>
public sealed class Supplier : TenantAggregateRoot
{
    public string Name { get; private set; } = default!;
    public string Code { get; private set; } = default!;
    public string? ContactEmail { get; private set; }
    public string? ContactPhone { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Supplier() { }

    public static Supplier Create(Guid companyId, string name, string code, string? contactEmail = null, string? contactPhone = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Supplier name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Supplier code is required.", nameof(code));
        if (contactEmail is not null && !contactEmail.Contains('@'))
            throw new ArgumentException("Contact email must be a valid email address.", nameof(contactEmail));

        var supplier = new Supplier
        {
            CompanyId = companyId,
            Name = name.Trim(),
            Code = code.Trim().ToUpperInvariant(),
            ContactEmail = contactEmail?.Trim().ToLowerInvariant(),
            ContactPhone = contactPhone,
        };

        supplier.Raise(new SupplierCreated(supplier.Id, companyId, supplier.Code));
        return supplier;
    }

    /// <summary>Updates the mutable master-data fields captured at Create time. Code and CompanyId are the tenant-scoped business key and stay immutable after creation.</summary>
    public void UpdateDetails(string name, string? contactEmail, string? contactPhone)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Supplier name is required.", nameof(name));
        if (contactEmail is not null && !contactEmail.Contains('@'))
            throw new ArgumentException("Contact email must be a valid email address.", nameof(contactEmail));

        Name = name.Trim();
        ContactEmail = contactEmail?.Trim().ToLowerInvariant();
        ContactPhone = contactPhone;
    }

    public void Deactivate() => IsActive = false;
}
