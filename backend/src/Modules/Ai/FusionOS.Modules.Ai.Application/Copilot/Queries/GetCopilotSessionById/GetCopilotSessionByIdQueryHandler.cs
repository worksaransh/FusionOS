using FusionOS.Modules.Ai.Application.Copilot.Contracts;
using MediatR;

namespace FusionOS.Modules.Ai.Application.Copilot.Queries.GetCopilotSessionById;

public sealed class GetCopilotSessionByIdQueryHandler : IRequestHandler<GetCopilotSessionByIdQuery, CopilotSessionDto>
{
    private readonly ICopilotSessionRepository _repository;

    public GetCopilotSessionByIdQueryHandler(ICopilotSessionRepository repository) => _repository = repository;

    public async Task<CopilotSessionDto> Handle(GetCopilotSessionByIdQuery request, CancellationToken cancellationToken)
    {
        var session = await _repository.GetByIdAsync(request.CompanyId, request.SessionId, cancellationToken)
            ?? throw new KeyNotFoundException($"Copilot session '{request.SessionId}' was not found.");

        return CopilotSessionMapper.ToDto(session);
    }
}
