using FusionOS.Modules.Hrms.Application.Employees.Contracts;
using FusionOS.Modules.Hrms.Domain.Onboarding;
using MediatR;

namespace FusionOS.Modules.Hrms.Application.Offboarding.Commands.CreateOffboardingWorkflow;

public sealed class CreateOffboardingWorkflowCommandHandler : IRequestHandler<CreateOffboardingWorkflowCommand, EmployeeWorkflowDto>
{
    private readonly IEmployeeWorkflowRepository _workflowRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateOffboardingWorkflowCommandHandler(IEmployeeWorkflowRepository workflowRepository, IEmployeeRepository employeeRepository, IUnitOfWork unitOfWork)
    {
        _workflowRepository = workflowRepository;
        _employeeRepository = employeeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<EmployeeWorkflowDto> Handle(CreateOffboardingWorkflowCommand request, CancellationToken cancellationToken)
    {
        // EmployeeId is a same-module reference — validated here, same convention CreateLeaveRequestCommandHandler uses for its own EmployeeId.
        if (!await _employeeRepository.ExistsAsync(request.CompanyId, request.EmployeeId, cancellationToken))
            throw new KeyNotFoundException($"Employee '{request.EmployeeId}' was not found.");

        var workflow = EmployeeWorkflow.Create(request.CompanyId, request.EmployeeId, WorkflowKind.Offboarding, request.TaskTitles, DateTimeOffset.UtcNow);

        await _workflowRepository.AddAsync(workflow, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return EmployeeWorkflowMapper.ToDto(workflow);
    }
}
