using FusionOS.Modules.Core.Domain.Comments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Core.Infrastructure.Persistence.Configurations;

/// <summary>
/// Comment isn't exposed as a DbSet&lt;Comment&gt; property on CoreDbContext
/// yet (a parallel change is landing there — see CoreModule.cs/CoreDbContext.cs
/// remediation notes) — EF Core still includes it in the model because
/// CoreDbContext.OnModelCreating calls ApplyConfigurationsFromAssembly, which
/// picks up this IEntityTypeConfiguration&lt;Comment&gt; regardless of
/// whether a DbSet property exists. CommentRepository accesses it via
/// context.Set&lt;Comment&gt;() for the same reason.
/// </summary>
public sealed class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.ToTable("comments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EntityType).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Body).HasMaxLength(Comment.MaxBodyLength).IsRequired();
        builder.UseXminAsConcurrencyToken(); // Postgres system column, not the app-level RowVersion byte[] (04_DATABASE_GUIDELINES.md) — same as ApprovalRequestConfiguration.
        builder.Ignore(x => x.RowVersion);
        builder.Ignore(x => x.DomainEvents);
        builder.HasIndex(x => new { x.CompanyId, x.EntityType, x.EntityId });
    }
}
