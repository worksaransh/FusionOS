using FusionOS.Modules.Crm.Application.Tasks.Contracts;
using MediatR;

namespace FusionOS.Modules.Crm.Application.Tasks.Queries.GetFollowUpTaskById;

public sealed class GetFollowUpTaskByIdQueryHandler : IRequestHandler<GetFollowUpTaskByIdQuery, FollowUpTaskDto>
{
    private readonly IFollowUpTaskRepository _repository;

    public GetFollowUpTaskByIdQueryHandler(IFollowUpTaskRepository repository) => _repository = repository;

    public async Task<FollowUpTaskDto> Handle(GetFollowUpTaskByIdQuery request, CancellationToken cancellationToken)
    {
        var task = await _repository.GetByIdAsync(request.CompanyId, request.TaskId, cancellationToken)
            ?? throw new KeyNotFoundException($"Follow-up task '{request.TaskId}' was not found.");

        return FollowUpTaskMapper.ToDto(task);
    }
}
