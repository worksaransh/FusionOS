using FusionOS.Modules.Core.Domain.Workflow;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Core.Infrastructure.Persistence.Configurations;

public sealed class ApprovalStepConfiguration : IEntityTypeConfiguration<ApprovalStep>
{
    public void Configure(EntityTypeBuilder<ApprovalStep> builder)
    {
        builder.ToTable("approval_steps");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Decision).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.Comments).HasMaxLength(2000);
        builder.HasIndex(x => new { x.ApproverUserId, x.Decision });
    }
}
