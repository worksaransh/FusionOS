using FusionOS.BuildingBlocks.Infrastructure.Persistence;
using FusionOS.Modules.Sales.Domain.Commissions;
using FusionOS.Modules.Sales.Domain.CreditNotes;
using FusionOS.Modules.Sales.Domain.Dispatches;
using FusionOS.Modules.Sales.Domain.Invoices;
using FusionOS.Modules.Sales.Domain.PriceLists;
using FusionOS.Modules.Sales.Domain.Quotations;
using FusionOS.Modules.Sales.Domain.SalesOrders;
using FusionOS.SharedKernel.Context;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Sales.Infrastructure.Persistence;

/// <summary>Owns the "sales" schema (04_DATABASE_GUIDELINES.md §1).</summary>
public sealed class SalesDbContext : BaseDbContext
{
    public SalesDbContext(DbContextOptions<SalesDbContext> options, ICurrentUserContext currentUser)
        : base(options, currentUser) { }

    public DbSet<Domain.Customers.Customer> Customers => Set<Domain.Customers.Customer>();
    public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<Dispatch> Dispatches => Set<Dispatch>();
    public DbSet<CreditNote> CreditNotes => Set<CreditNote>();
    public DbSet<Quotation> Quotations => Set<Quotation>();
    public DbSet<PriceList> PriceLists => Set<PriceList>();
    public DbSet<SalesCommissionRate> SalesCommissionRates => Set<SalesCommissionRate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("sales");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SalesDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
