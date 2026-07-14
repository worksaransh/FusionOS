using FusionOS.BuildingBlocks.Infrastructure.Persistence;
using FusionOS.Modules.Core.Domain.Audit;
using FusionOS.Modules.Core.Domain.Companies;
using FusionOS.Modules.Core.Domain.Identity;
using FusionOS.Modules.Core.Domain.Notifications;
using FusionOS.Modules.Core.Domain.Organizations;
using FusionOS.SharedKernel.Context;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Core.Infrastructure.Persistence;

/// <summary>
/// The Core module's own schema ("core"). Per 03_SYSTEM_ARCHITECTURE.md §2, no
/// other module's DbContext may reference these tables directly — cross-module
/// access is via the published Core.Application contracts only.
/// </summary>
public sealed class CoreDbContext : BaseDbContext
{
    public CoreDbContext(DbContextOptions<CoreDbContext> options, ICurrentUserContext currentUser) : base(options, currentUser) { }

    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserCompanyRole> UserCompanyRoles => Set<UserCompanyRole>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("core");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CoreDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
