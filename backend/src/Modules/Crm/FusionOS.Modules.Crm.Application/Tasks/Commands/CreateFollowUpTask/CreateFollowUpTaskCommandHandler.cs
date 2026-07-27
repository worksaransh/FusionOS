using FusionOS.Modules.Crm.Application.Leads.Contracts;
using FusionOS.Modules.Crm.Application.Tasks.Contracts;
using FusionOS.Modules.Crm.Domain.Tasks;
using MediatR;

namespace FusionOS.Modules.Crm.Application.Tasks.Commands.CreateFollowUpTask;

public sealed class CreateFollowUpTaskCommandHandler : IRequestHandler<CreateFollowUpTaskCommand, FollowUpTaskDto>
{
    private readonly IFollowUpTaskRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateFollowUpTaskCommandHandler(IFollowUpTaskRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<FollowUpTaskDto> Handle(CreateFollowUpTaskCommand request, CancellationToken cancellationToken)
    {
        var task = FollowUpTask.Create(
            request.CompanyId, request.Title, request.Description, request.DueDate,
            request.EntityType, request.EntityId, request.AssignedToUserId);

        await _repository.AddAsync(task, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return FollowUpTaskMapper.ToDto(task);
    }
}
