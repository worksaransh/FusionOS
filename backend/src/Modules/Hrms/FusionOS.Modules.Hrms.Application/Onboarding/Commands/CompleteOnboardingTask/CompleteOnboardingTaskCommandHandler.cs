using FusionOS.Modules.Hrms.Application.Employees.Contracts;
using FusionOS.Modules.Hrms.Domain.Onboarding;
using MediatR;

namespace FusionOS.Modules.Hrms.Application.Onboarding.Commands.CompleteOnboardingTask;

public sealed class CompleteOnboardingTaskCommandHandler : IRequestHandler<CompleteOnboardingTaskCommand, EmployeeWorkflowDto>
{
    private readonly IEmployeeWorkflowRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CompleteOnboardingTaskCommandHandler(IEmployeeWorkflowRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<EmployeeWorkflowDto> Handle(CompleteOnboardingTaskCommand request, CancellationToken cancellationToken)
    {
        var workflow = await _repository.GetByIdAsync(request.CompanyId, request.WorkflowId, WorkflowKind.Onboarding, cancellationToken)
            ?? throw new KeyNotFoundException($"Onboarding workflow '{request.WorkflowId}' was not found.");

        workflow.CompleteTask(request.TaskId, DateTimeOffset.UtcNow, request.Notes);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return EmployeeWorkflowMapper.ToDto(workflow);
    }
}
