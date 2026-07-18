using FusionOS.SharedKernel;
using FusionOS.Modules.Sales.Domain.Customers.Events;

namespace FusionOS.Modules.Sales.Domain.Customers;

/// <summary>
/// The anchor aggregate for Sales (05_MODULE_ROADMAP.md Phase 1). Quotations,
/// Sales Orders, Invoices, and price lists all reference a Customer — this is
/// the narrow first cut: customer master data and credit limit.
/// </summary>
public sealed class Customer : TenantAggregateRoot
{
    public string Name { get; private set; } = default!;
    public string Code { get; private set; } = default!;
    public string? ContactEmail { get; private set; }
    public decimal CreditLimit { get; private set; }
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Optional same-module reference to a PriceList (Phase 10 item 10's
    /// "multiple price lists per customer segment" — see PriceList's own doc
    /// comment for why this is per-Customer rather than per-segment). Validated
    /// via IPriceListRepository.ExistsAsync in AssignPriceListCommandHandler,
    /// unlike the opaque cross-module ProductId elsewhere in Sales.
    /// </summary>
    public Guid? PriceListId { get; private set; }

    private Customer() { }

    public static Customer Create(Guid companyId, string name, string code, string? contactEmail = null, decimal creditLimit = 0m)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Customer name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Customer code is required.", nameof(code));
        if (contactEmail is not null && !contactEmail.Contains('@'))
            throw new ArgumentException("Contact email must be a valid email address.", nameof(contactEmail));
        if (creditLimit < 0)
            throw new ArgumentException("Credit limit cannot be negative.", nameof(creditLimit));

        var customer = new Customer
        {
            CompanyId = companyId,
            Name = name.Trim(),
            Code = code.Trim().ToUpperInvariant(),
            ContactEmail = contactEmail?.Trim().ToLowerInvariant(),
            CreditLimit = creditLimit,
        };

        customer.Raise(new CustomerCreated(customer.Id, companyId, customer.Code));
        return customer;
    }

    public void Deactivate() => IsActive = false;

    /// <summary>
    /// Covers the same descriptive fields Create accepts, excluding Code — the
    /// customer code is the business key (uniqueness-checked at creation) and
    /// stays immutable after creation, matching Product.Sku/Warehouse.Code.
    /// </summary>
    public void UpdateDetails(string name, string? contactEmail, decimal creditLimit)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Customer name is required.", nameof(name));
        if (contactEmail is not null && !contactEmail.Contains('@'))
            throw new ArgumentException("Contact email must be a valid email address.", nameof(contactEmail));
        if (creditLimit < 0)
            throw new ArgumentException("Credit limit cannot be negative.", nameof(creditLimit));

        Name = name.Trim();
        ContactEmail = contactEmail?.Trim().ToLowerInvariant();
        CreditLimit = creditLimit;
    }

    /// <summary>
    /// Sets or clears (pass null) which PriceList this customer's Sales Orders
    /// should be priced from. Existence of a non-null id is validated by
    /// AssignPriceListCommandHandler via IPriceListRepository.ExistsAsync before
    /// this is called — the domain method itself only records the fact, same
    /// division of responsibility as every other same-module reference in this
    /// codebase (e.g. CreateCreditNoteCommandHandler validating InvoiceId before
    /// CreditNote.Create is ever called).
    /// </summary>
    public void AssignPriceList(Guid? priceListId) => PriceListId = priceListId;
}
