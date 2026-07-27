using FusionOS.Modules.Hrms.Domain.Onboarding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Hrms.Infrastructure.Persistence.Configurations;

/// <summary>
/// EmployeeWorkflow (and its child WorkflowTask) has no dedicated DbSet property on HrmsDbContext
/// — that file was left untouched for this pass (owned by the orchestrating session). That's not
/// a problem: HrmsDbContext.OnModelCreating already calls
/// modelBuilder.ApplyConfigurationsFromAssembly(...), and ModelBuilder.ApplyConfiguration&lt;T&gt;
/// implicitly adds an entity type to the model the first time it's configured — a DbSet property
/// is only a query-surface convenience, not a model-registration requirement. EmployeeWorkflowRepository
/// queries this type via context.Set&lt;EmployeeWorkflow&gt;() instead of a named DbSet. Adding
/// DbSet&lt;EmployeeWorkflow&gt; Employees... (properties) to HrmsDbContext later is a
/// safe, purely-additive follow-up if a named DbSet is ever wanted for readability.
/// </summary>
public sealed class EmployeeWorkflowConfiguration : IEntityTypeConfiguration<EmployeeWorkflow>
{
    public void Configure(EntityTypeBuilder<EmployeeWorkflow> builder)
    {
        builder.ToTable("employee_workflows");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Kind).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.UseXminAsConcurrencyToken(); // Postgres system column, not the app-level RowVersion byte[] (04_DATABASE_GUIDELINES.md)
        builder.Ignore(x => x.RowVersion);
        builder.Ignore(x => x.DomainEvents);
        // No FK constraint to Employee: existence is validated in the command handler, same
        // convention as every other same-module reference in this codebase (e.g. LeaveRequest.EmployeeId).
        builder.HasIndex(x => new { x.CompanyId, x.EmployeeId, x.Kind });
        builder.HasIndex(x => new { x.CompanyId, x.Kind, x.Status });

        builder.HasMany(x => x.Tasks)
            .WithOne()
            .HasForeignKey("EmployeeWorkflowId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Tasks).HasField("_tasks").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class WorkflowTaskConfiguration : IEntityTypeConfiguration<WorkflowTask>
{
    public void Configure(EntityTypeBuilder<WorkflowTask> builder)
    {
        builder.ToTable("employee_workflow_tasks");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(1000);
    }
}
