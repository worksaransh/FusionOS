using FusionOS.BuildingBlocks.Infrastructure.Persistence;
using FusionOS.Modules.Sales.Domain.Dispatches;
using FusionOS.Modules.Sales.Domain.Invoices;
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("sales");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SalesDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
