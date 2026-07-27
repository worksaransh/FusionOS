using FusionOS.Modules.Ai.Application.Copilot.Contracts;
using FusionOS.Modules.Ai.Domain.Copilot;
using FusionOS.Modules.Ai.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Ai.Infrastructure.Repositories;

public sealed class CopilotSessionRepository : ICopilotSessionRepository
{
    private readonly AiDbContext _context;

    public CopilotSessionRepository(AiDbContext context) => _context = context;

    public Task<CopilotSession?> GetByIdAsync(Guid companyId, Guid sessionId, CancellationToken cancellationToken = default) =>
        _context.CopilotSessions
            .Include(s => s.Messages)
            .FirstOrDefaultAsync(s => s.CompanyId == companyId && s.Id == sessionId, cancellationToken);

    public async Task AddAsync(CopilotSession session, CancellationToken cancellationToken = default) =>
        await _context.CopilotSessions.AddAsync(session, cancellationToken);

    public async Task<IReadOnlyList<CopilotSession>> ListForUserAsync(Guid companyId, Guid userId, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await Filtered(companyId, userId)
            .Include(s => s.Messages)
            .OrderByDescending(s => s.LastMessageAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountForUserAsync(Guid companyId, Guid userId, CancellationToken cancellationToken = default) =>
        Filtered(companyId, userId).CountAsync(cancellationToken);

    private IQueryable<CopilotSession> Filtered(Guid companyId, Guid userId) =>
        _context.CopilotSessions.Where(s => s.CompanyId == companyId && s.UserId == userId);
}
