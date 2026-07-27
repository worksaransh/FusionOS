namespace FusionOS.Modules.Maintenance.Application.ChecklistTemplates.Contracts;

public interface IChecklistTemplateRepository
{
    Task<Domain.ChecklistTemplates.ChecklistTemplate?> GetByIdAsync(Guid companyId, Guid checklistTemplateId, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.ChecklistTemplates.ChecklistTemplate template, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.ChecklistTemplates.ChecklistTemplate>> ListAsync(Guid companyId, string? search, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, string? search, CancellationToken cancellationToken = default);
}
