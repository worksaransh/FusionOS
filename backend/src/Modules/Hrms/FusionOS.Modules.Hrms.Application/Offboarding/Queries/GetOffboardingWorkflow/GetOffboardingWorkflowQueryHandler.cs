using FusionOS.Modules.Hrms.Application.Employees.Contracts;
using FusionOS.Modules.Hrms.Domain.Onboarding;
using MediatR;

namespace FusionOS.Modules.Hrms.Application.Offboarding.Queries.GetOffboardingWorkflow;

public sealed class GetOffboardingWorkflowQueryHandler : IRequestHandler<GetOffboardingWorkflowQuery, EmployeeWorkflowDto>
{
    private readonly IEmployeeWorkflowRepository _repository;

    public GetOffboardingWorkflowQueryHandler(IEmployeeWorkflowRepository repository) => _repository = repository;

    public async Task<EmployeeWorkflowDto> Handle(GetOffboardingWorkflowQuery request, CancellationToken cancellationToken)
    {
        var workflow = await _repository.GetByIdAsync(request.CompanyId, request.WorkflowId, WorkflowKind.Offboarding, cancellationToken)
            ?? throw new KeyNotFoundException($"Offboarding workflow '{request.WorkflowId}' was not found.");

        return EmployeeWorkflowMapper.ToDto(workflow);
    }
}
