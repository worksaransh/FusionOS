using FusionOS.SharedKernel;
using FusionOS.Modules.Sales.Domain.SalesOrders.Events;

namespace FusionOS.Modules.Sales.Domain.SalesOrders;

/// <summary>
/// The next slice after Customer (05_MODULE_ROADMAP.md Phase 1). Quotation,
/// Invoice, Dispatch, Returns, and Credit Notes come later; this slice covers
/// Sales Order creation and confirmation.
/// </summary>
public sealed class SalesOrder : TenantAggregateRoot
{
    private readonly List<SalesOrderLine> _lines = new();

    public Guid CustomerId { get; private set; }
    public SalesOrderStatus Status { get; private set; }
    public DateTimeOffset OrderDate { get; private set; }
    public IReadOnlyList<SalesOrderLine> Lines => _lines.AsReadOnly();
    public decimal TotalAmount => _lines.Sum(l => l.LineTotal);

    private SalesOrder() { }

    public static SalesOrder Create(Guid companyId, Guid customerId, IReadOnlyCollection<SalesOrderLineInput> lines)
    {
        if (customerId == Guid.Empty)
            throw new ArgumentException("Customer id is required.", nameof(customerId));
        if (lines is null || lines.Count == 0)
            throw new ArgumentException("A sales order must have at least one line.", nameof(lines));

        var order = new SalesOrder
        {
            CompanyId = companyId,
            CustomerId = customerId,
            Status = SalesOrderStatus.Draft,
            OrderDate = DateTimeOffset.UtcNow,
        };

        foreach (var line in lines)
            order._lines.Add(SalesOrderLine.Create(line.ProductId, line.Quantity, line.UnitPrice, line.DiscountPercentage));

        order.Raise(new SalesOrderCreated(order.Id, companyId, customerId, order.TotalAmount));
        return order;
    }

    /// <summary>Raises SalesOrderConfirmed — the event Inventory (reservation), Warehouse (pick task), and Finance (AR) consume per 03_SYSTEM_ARCHITECTURE.md §4.2's event catalog, once cross-module consumption is wired up.</summary>
    public void Confirm()
    {
        if (Status != SalesOrderStatus.Draft)
            throw new InvalidOperationException($"Only a Draft sales order can be confirmed (current status: {Status}).");

        Status = SalesOrderStatus.Confirmed;
        Raise(new SalesOrderConfirmed(Id, CompanyId, CustomerId, TotalAmount));
    }
}
