using FusionOS.Modules.Ai.Domain.Copilot;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Ai.Infrastructure.Persistence.Configurations;

public sealed class CopilotSessionConfiguration : IEntityTypeConfiguration<CopilotSession>
{
    public void Configure(EntityTypeBuilder<CopilotSession> builder)
    {
        builder.ToTable("copilot_sessions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.UseXminAsConcurrencyToken(); // Postgres system column, not the app-level RowVersion byte[] (04_DATABASE_GUIDELINES.md)
        builder.Ignore(x => x.RowVersion);
        builder.Ignore(x => x.DomainEvents);
        builder.HasIndex(x => new { x.CompanyId, x.UserId });

        builder.HasMany(x => x.Messages)
            .WithOne()
            .HasForeignKey("CopilotSessionId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Messages).HasField("_messages").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class CopilotMessageConfiguration : IEntityTypeConfiguration<CopilotMessage>
{
    public void Configure(EntityTypeBuilder<CopilotMessage> builder)
    {
        builder.ToTable("copilot_messages");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Role).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.Content).HasMaxLength(4000).IsRequired();
    }
}
