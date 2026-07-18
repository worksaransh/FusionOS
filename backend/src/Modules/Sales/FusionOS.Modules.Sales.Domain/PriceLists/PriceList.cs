using FusionOS.SharedKernel;
using FusionOS.Modules.Sales.Domain.PriceLists.Events;

namespace FusionOS.Modules.Sales.Domain.PriceLists;

/// <summary>
/// "Multiple price lists per customer segment" (docs/IMPLEMENTATION_PLAN.md Phase
/// 10 item 10, the pricing/discount-engine half — the discount-per-line half
/// lives on SalesOrderLine/SalesOrderLineInput instead). A PriceList is a named
/// set of per-product override prices; a Customer optionally points at one via
/// Customer.AssignPriceList. There is no "customer segment" concept modeled
/// anywhere in this codebase yet, so a PriceList is assigned directly to
/// individual Customers rather than to a segment that doesn't exist — the
/// smallest bounded slice that still satisfies the item's intent without
/// inventing a new unstated segmentation model.
/// </summary>
public sealed class PriceList : TenantAggregateRoot
{
    private readonly List<PriceListEntry> _entries = new();

    public string Name { get; private set; } = default!;
    public bool IsActive { get; private set; } = true;
    public IReadOnlyList<PriceListEntry> Entries => _entries.AsReadOnly();

    private PriceList() { }

    public static PriceList Create(Guid companyId, string name, IReadOnlyCollection<PriceListEntryInput> entries)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Price list name is required.", nameof(name));
        if (entries is null || entries.Count == 0)
            throw new ArgumentException("A price list must have at least one entry.", nameof(entries));

        var priceList = new PriceList
        {
            CompanyId = companyId,
            Name = name.Trim(),
        };

        foreach (var entry in entries)
            priceList._entries.Add(PriceListEntry.Create(entry.ProductId, entry.UnitPrice));

        priceList.Raise(new PriceListCreated(priceList.Id, companyId, priceList.Name));
        return priceList;
    }

    public void Deactivate() => IsActive = false;
}
