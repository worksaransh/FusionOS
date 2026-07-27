using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Ai.Application.Copilot.Contracts;
using MediatR;

namespace FusionOS.Modules.Ai.Application.Copilot.Queries.ListCopilotSessionsForUser;

public sealed class ListCopilotSessionsForUserQueryHandler : IRequestHandler<ListCopilotSessionsForUserQuery, PagedResult<CopilotSessionDto>>
{
    private readonly ICopilotSessionRepository _repository;

    public ListCopilotSessionsForUserQueryHandler(ICopilotSessionRepository repository) => _repository = repository;

    public async Task<PagedResult<CopilotSessionDto>> Handle(ListCopilotSessionsForUserQuery request, CancellationToken cancellationToken)
    {
        var sessions = await _repository.ListForUserAsync(request.CompanyId, request.UserId, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountForUserAsync(request.CompanyId, request.UserId, cancellationToken);

        var dtos = sessions.Select(CopilotSessionMapper.ToDto).ToList();

        return new PagedResult<CopilotSessionDto>(dtos, request.Page, request.PageSize, total);
    }
}
