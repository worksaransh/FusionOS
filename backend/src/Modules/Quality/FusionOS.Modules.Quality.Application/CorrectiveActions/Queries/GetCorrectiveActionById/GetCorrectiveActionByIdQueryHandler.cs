using FusionOS.Modules.Quality.Application.CorrectiveActions.Contracts;
using MediatR;

namespace FusionOS.Modules.Quality.Application.CorrectiveActions.Queries.GetCorrectiveActionById;

public sealed class GetCorrectiveActionByIdQueryHandler : IRequestHandler<GetCorrectiveActionByIdQuery, CorrectiveActionDto>
{
    private readonly ICorrectiveActionRepository _repository;

    public GetCorrectiveActionByIdQueryHandler(ICorrectiveActionRepository repository) => _repository = repository;

    public async Task<CorrectiveActionDto> Handle(GetCorrectiveActionByIdQuery request, CancellationToken cancellationToken)
    {
        var correctiveAction = await _repository.GetByIdAsync(request.CompanyId, request.CorrectiveActionId, cancellationToken)
            ?? throw new KeyNotFoundException($"Corrective action '{request.CorrectiveActionId}' was not found.");

        return CorrectiveActionMapper.ToDto(correctiveAction);
    }
}
