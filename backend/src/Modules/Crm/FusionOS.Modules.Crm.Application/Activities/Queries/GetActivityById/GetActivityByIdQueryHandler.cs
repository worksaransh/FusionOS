using FusionOS.Modules.Crm.Application.Activities.Contracts;
using MediatR;

namespace FusionOS.Modules.Crm.Application.Activities.Queries.GetActivityById;

public sealed class GetActivityByIdQueryHandler : IRequestHandler<GetActivityByIdQuery, ActivityDto>
{
    private readonly IActivityRepository _repository;

    public GetActivityByIdQueryHandler(IActivityRepository repository) => _repository = repository;

    public async Task<ActivityDto> Handle(GetActivityByIdQuery request, CancellationToken cancellationToken)
    {
        var activity = await _repository.GetByIdAsync(request.CompanyId, request.ActivityId, cancellationToken)
            ?? throw new KeyNotFoundException($"Activity '{request.ActivityId}' was not found.");

        return ActivityMapper.ToDto(activity);
    }
}
