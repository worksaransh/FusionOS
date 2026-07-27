using FusionOS.Modules.Hrms.Application.Employees.Contracts;
using FusionOS.Modules.Hrms.Domain.Onboarding;
using MediatR;

namespace FusionOS.Modules.Hrms.Application.Onboarding.Queries.GetOnboardingWorkflow;

public sealed class GetOnboardingWorkflowQueryHandler : IRequestHandler<GetOnboardingWorkflowQuery, EmployeeWorkflowDto>
{
    private readonly IEmployeeWorkflowRepository _repository;

    public GetOnboardingWorkflowQueryHandler(IEmployeeWorkflowRepository repository) => _repository = repository;

    public async Task<EmployeeWorkflowDto> Handle(GetOnboardingWorkflowQuery request, CancellationToken cancellationToken)
    {
        var workflow = await _repository.GetByIdAsync(request.CompanyId, request.WorkflowId, WorkflowKind.Onboarding, cancellationToken)
            ?? throw new KeyNotFoundException($"Onboarding workflow '{request.WorkflowId}' was not found.");

        return EmployeeWorkflowMapper.ToDto(workflow);
    }
}
