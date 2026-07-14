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
}
