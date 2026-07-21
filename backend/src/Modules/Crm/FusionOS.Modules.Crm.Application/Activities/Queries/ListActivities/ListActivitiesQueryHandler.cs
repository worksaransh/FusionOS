using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Crm.Application.Activities.Contracts;
using MediatR;

namespace FusionOS.Modules.Crm.Application.Activities.Queries.ListActivities;

public sealed class ListActivitiesQueryHandler : IRequestHandler<ListActivitiesQuery, PagedResult<ActivityDto>>
{
    private readonly IActivityRepository _repository;

    public ListActivitiesQueryHandler(IActivityRepository repository) => _repository = repository;

    public async Task<PagedResult<ActivityDto>> Handle(ListActivitiesQuery request, CancellationToken cancellationToken)
    {
        var activities = await _repository.ListAsync(request.CompanyId, request.EntityType, request.EntityId, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, request.EntityType, request.EntityId, cancellationToken);

        var dtos = activities.Select(ActivityMapper.ToDto).ToList();

        return new PagedResult<ActivityDto>(dtos, request.Page, request.PageSize, total);
    }
}
