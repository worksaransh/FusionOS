namespace FusionOS.Modules.Core.Application.Auth;

/// <summary>
/// The single source of truth for every permission code enforced anywhere in
/// FusionOS via IRequirePermission (07_SECURITY.md §2). New modules add their
/// codes here so GetOrCreateCompanyOwnerRoleAsync can seed them - otherwise a
/// freshly-bootstrapped company's "Owner" role would silently be missing grants
/// for whatever was added after the role was first created.
/// </summary>
public static class PermissionCatalog
{
    public static readonly IReadOnlyList<(string Module, string Code, string Description)> All = new (string, string, string)[]
    {
        ("core", "core.user.register", "Register (invite) a new user into this company"),
        ("finance", "finance.account.create", "Create a chart-of-accounts account"),
        ("finance", "finance.journal-entry.create", "Create a draft journal entry"),
        ("finance", "finance.journal-entry.post", "Post a journal entry to the ledger"),
        ("inventory", "inventory.product.create", "Create a product"),
        ("inventory", "inventory.stock.adjust", "Adjust stock-on-hand"),
        ("procurement", "procurement.purchase-order.approve", "Approve a purchase order"),
        ("procurement", "procurement.purchase-order.create", "Create a purchase order"),
        ("procurement", "procurement.supplier.create", "Create a supplier"),
        ("sales", "sales.customer.create", "Create a customer"),
        ("sales", "sales.dispatch.create", "Create a dispatch"),
        ("sales", "sales.invoice.create", "Create a sales invoice"),
        ("sales", "sales.invoice.issue", "Issue a sales invoice"),
        ("sales", "sales.sales-order.confirm", "Confirm a sales order"),
        ("sales", "sales.sales-order.create", "Create a sales order"),
        ("warehouse", "warehouse.goods-receipt.create", "Create a goods receipt"),
        ("warehouse", "warehouse.warehouse.create", "Create a warehouse"),
        ("warehouse", "warehouse.zone.create", "Create a warehouse zone"),
    };
}
