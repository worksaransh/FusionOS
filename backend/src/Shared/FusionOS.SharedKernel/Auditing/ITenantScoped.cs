namespace FusionOS.SharedKernel.Auditing;

/// <summary>
/// Multi-company / multi-branch discriminator per 04_DATABASE_GUIDELINES.md §3
/// and §6. Enforced everywhere except the small set of entities that define the
/// tenant boundary itself (e.g. Company) — those are explicitly reviewed exceptions.
/// </summary>
public interface ITenantScoped
{
    Guid CompanyId { get; }
    Guid? BranchId { get; }
}
