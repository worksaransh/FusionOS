using FusionOS.Modules.Hrms.Application.Employees.Contracts;
using FusionOS.Modules.Hrms.Domain.Onboarding;
using MediatR;

namespace FusionOS.Modules.Hrms.Application.Offboarding.Commands.CompleteOffboardingTask;

public sealed class CompleteOffboardingTaskCommandHandler : IRequestHandler<CompleteOffboardingTaskCommand, EmployeeWorkflowDto>
{
    private readonly IEmployeeWorkflowRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CompleteOffboardingTaskCommandHandler(IEmployeeWorkflowRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<EmployeeWorkflowDto> Handle(CompleteOffboardingTaskCommand request, CancellationToken cancellationToken)
    {
        var workflow = await _repository.GetByIdAsync(request.CompanyId, request.WorkflowId, WorkflowKind.Offboarding, cancellationToken)
            ?? throw new KeyNotFoundException($"Offboarding workflow '{request.WorkflowId}' was not found.");

        workflow.CompleteTask(request.TaskId, DateTimeOffset.UtcNow, request.Notes);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return EmployeeWorkflowMapper.ToDto(workflow);
    }
}
