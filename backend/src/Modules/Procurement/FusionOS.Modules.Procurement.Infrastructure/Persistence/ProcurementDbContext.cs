using FusionOS.BuildingBlocks.Infrastructure.Persistence;
using FusionOS.Modules.Procurement.Domain.PurchaseOrders;
using FusionOS.Modules.Procurement.Domain.Rfqs;
using FusionOS.Modules.Procurement.Domain.SupplierContracts;
using FusionOS.Modules.Procurement.Domain.VendorReturns;
using FusionOS.SharedKernel.Context;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Procurement.Infrastructure.Persistence;

/// <summary>Owns the "procurement" schema (04_DATABASE_GUIDELINES.md §1).</summary>
public sealed class ProcurementDbContext : BaseDbContext
{
    public ProcurementDbContext(DbContextOptions<ProcurementDbContext> options, ICurrentUserContext currentUser)
        : base(options, currentUser) { }

    public DbSet<Domain.Suppliers.Supplier> Suppliers => Set<Domain.Suppliers.Supplier>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<RequestForQuotation> Rfqs => Set<RequestForQuotation>();
    public DbSet<SupplierContract> SupplierContracts => Set<SupplierContract>();
    public DbSet<VendorReturn> VendorReturns => Set<VendorReturn>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("procurement");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProcurementDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
