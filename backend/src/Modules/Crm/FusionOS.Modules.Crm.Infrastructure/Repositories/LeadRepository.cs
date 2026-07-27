using FusionOS.Modules.Crm.Application.Leads.Contracts;
using FusionOS.Modules.Crm.Domain.Leads;
using FusionOS.Modules.Crm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Crm.Infrastructure.Repositories;

public sealed class LeadRepository : ILeadRepository
{
    private readonly CrmDbContext _context;

    public LeadRepository(CrmDbContext context) => _context = context;

    public Task<Lead?> GetByIdAsync(Guid companyId, Guid leadId, CancellationToken cancellationToken = default) =>
        _context.Leads.FirstOrDefaultAsync(l => l.CompanyId == companyId && l.Id == leadId, cancellationToken);

    public async Task AddAsync(Lead lead, CancellationToken cancellationToken = default) =>
        await _context.Leads.AddAsync(lead, cancellationToken);

    public async Task<IReadOnlyList<Lead>> ListAsync(Guid companyId, string? search, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await Filtered(companyId, search)
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, string? search, CancellationToken cancellationToken = default) =>
        Filtered(companyId, search).CountAsync(cancellationToken);

    public async Task<(int TotalCount, int ConvertedCount)> GetConversionCountsAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        var totalCount = await _context.Leads.CountAsync(l => l.CompanyId == companyId, cancellationToken);
        var convertedCount = await _context.Leads.CountAsync(l => l.CompanyId == companyId && l.Status == LeadStatus.Converted, cancellationToken);
        return (totalCount, convertedCount);
    }

    public Task<bool> IsEmailAlreadyUsedAsync(Guid companyId, string normalizedEmail, CancellationToken cancellationToken = default) =>
        // ContactEmail is already stored trimmed + lower-cased by Lead.Create, so a plain
        // equality check against an already-normalized input is sufficient here.
        _context.Leads.AnyAsync(l => l.CompanyId == companyId && l.ContactEmail == normalizedEmail, cancellationToken);

    private IQueryable<Lead> Filtered(Guid companyId, string? search)
    {
        var query = _context.Leads.Where(l => l.CompanyId == companyId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(l => EF.Functions.ILike(l.Name, pattern) || (l.ContactEmail != null && EF.Functions.ILike(l.ContactEmail, pattern)));
        }

        return query;
    }
}
