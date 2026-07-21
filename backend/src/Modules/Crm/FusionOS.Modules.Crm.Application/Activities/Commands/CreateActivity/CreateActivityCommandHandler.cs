using FusionOS.Modules.Crm.Application.Activities.Contracts;
using FusionOS.Modules.Crm.Application.Leads.Contracts;
using FusionOS.Modules.Crm.Domain.Activities;
using MediatR;

namespace FusionOS.Modules.Crm.Application.Activities.Commands.CreateActivity;

public sealed class CreateActivityCommandHandler : IRequestHandler<CreateActivityCommand, ActivityDto>
{
    private readonly IActivityRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateActivityCommandHandler(IActivityRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ActivityDto> Handle(CreateActivityCommand request, CancellationToken cancellationToken)
    {
        // Safe unguarded parse — CreateActivityValidator's Enum.TryParse rule ran before
        // this handler (same division as Finance's CreateAccountCommandHandler.AccountType).
        var type = Enum.Parse<ActivityType>(request.Type);

        var activity = Domain.Activities.Activity.Log(request.CompanyId, request.EntityType, request.EntityId, type, request.Notes);

        await _repository.AddAsync(activity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ActivityMapper.ToDto(activity);
    }
}
