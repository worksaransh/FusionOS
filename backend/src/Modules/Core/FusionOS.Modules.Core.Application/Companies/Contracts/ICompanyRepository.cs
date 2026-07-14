using FusionOS.Modules.Core.Domain.Companies;

namespace FusionOS.Modules.Core.Application.Companies.Contracts;

public interface ICompanyRepository
{
    Task<Company?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Company company, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Company>> ListAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
}
