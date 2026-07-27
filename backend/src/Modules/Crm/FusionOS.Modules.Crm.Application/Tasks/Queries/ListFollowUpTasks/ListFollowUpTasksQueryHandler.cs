using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Crm.Application.Tasks.Contracts;
using MediatR;

namespace FusionOS.Modules.Crm.Application.Tasks.Queries.ListFollowUpTasks;

public sealed class ListFollowUpTasksQueryHandler : IRequestHandler<ListFollowUpTasksQuery, PagedResult<FollowUpTaskDto>>
{
    private readonly IFollowUpTaskRepository _repository;

    public ListFollowUpTasksQueryHandler(IFollowUpTaskRepository repository) => _repository = repository;

    public async Task<PagedResult<FollowUpTaskDto>> Handle(ListFollowUpTasksQuery request, CancellationToken cancellationToken)
    {
        var tasks = await _repository.ListAsync(
            request.CompanyId, request.AssignedToUserId, request.EntityType, request.EntityId,
            request.IsCompleted, request.OverdueOnly, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(
            request.CompanyId, request.AssignedToUserId, request.EntityType, request.EntityId,
            request.IsCompleted, request.OverdueOnly, cancellationToken);

        var dtos = tasks.Select(FollowUpTaskMapper.ToDto).ToList();

        return new PagedResult<FollowUpTaskDto>(dtos, request.Page, request.PageSize, total);
    }
}
