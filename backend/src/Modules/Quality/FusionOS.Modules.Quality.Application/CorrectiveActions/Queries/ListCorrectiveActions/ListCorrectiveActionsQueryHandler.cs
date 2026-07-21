using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Quality.Application.CorrectiveActions.Contracts;
using MediatR;

namespace FusionOS.Modules.Quality.Application.CorrectiveActions.Queries.ListCorrectiveActions;

public sealed class ListCorrectiveActionsQueryHandler : IRequestHandler<ListCorrectiveActionsQuery, PagedResult<CorrectiveActionDto>>
{
    private readonly ICorrectiveActionRepository _repository;

    public ListCorrectiveActionsQueryHandler(ICorrectiveActionRepository repository) => _repository = repository;

    public async Task<PagedResult<CorrectiveActionDto>> Handle(ListCorrectiveActionsQuery request, CancellationToken cancellationToken)
    {
        var correctiveActions = await _repository.ListAsync(request.CompanyId, request.NonConformanceReportId, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, request.NonConformanceReportId, cancellationToken);

        var dtos = correctiveActions.Select(CorrectiveActionMapper.ToDto).ToList();

        return new PagedResult<CorrectiveActionDto>(dtos, request.Page, request.PageSize, total);
    }
}
