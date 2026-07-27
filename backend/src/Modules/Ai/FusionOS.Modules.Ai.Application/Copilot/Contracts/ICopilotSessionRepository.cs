using FusionOS.Modules.Ai.Domain.Copilot;

namespace FusionOS.Modules.Ai.Application.Copilot.Contracts;

public interface ICopilotSessionRepository
{
    Task<CopilotSession?> GetByIdAsync(Guid companyId, Guid sessionId, CancellationToken cancellationToken = default);
    Task AddAsync(CopilotSession session, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CopilotSession>> ListForUserAsync(Guid companyId, Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountForUserAsync(Guid companyId, Guid userId, CancellationToken cancellationToken = default);
}
