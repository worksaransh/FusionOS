using FusionOS.Modules.Crm.Application.Leads.Contracts;
using FusionOS.Modules.Crm.Application.Tasks.Contracts;
using MediatR;

namespace FusionOS.Modules.Crm.Application.Tasks.Commands.CompleteFollowUpTask;

public sealed class CompleteFollowUpTaskCommandHandler : IRequestHandler<CompleteFollowUpTaskCommand, FollowUpTaskDto>
{
    private readonly IFollowUpTaskRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CompleteFollowUpTaskCommandHandler(IFollowUpTaskRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<FollowUpTaskDto> Handle(CompleteFollowUpTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _repository.GetByIdAsync(request.CompanyId, request.TaskId, cancellationToken)
            ?? throw new KeyNotFoundException($"Follow-up task '{request.TaskId}' was not found.");

        task.Complete();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return FollowUpTaskMapper.ToDto(task);
    }
}
