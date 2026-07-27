using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Hrms.Application.Employees.Contracts;
using FusionOS.Modules.Hrms.Domain.Onboarding;
using MediatR;

namespace FusionOS.Modules.Hrms.Application.Onboarding.Queries.ListOnboardingWorkflows;

public sealed class ListOnboardingWorkflowsQueryHandler : IRequestHandler<ListOnboardingWorkflowsQuery, PagedResult<EmployeeWorkflowDto>>
{
    private readonly IEmployeeWorkflowRepository _repository;

    public ListOnboardingWorkflowsQueryHandler(IEmployeeWorkflowRepository repository) => _repository = repository;

    public async Task<PagedResult<EmployeeWorkflowDto>> Handle(ListOnboardingWorkflowsQuery request, CancellationToken cancellationToken)
    {
        var workflows = await _repository.ListAsync(request.CompanyId, WorkflowKind.Onboarding, request.EmployeeId, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, WorkflowKind.Onboarding, request.EmployeeId, cancellationToken);

        return new PagedResult<EmployeeWorkflowDto>(workflows.Select(EmployeeWorkflowMapper.ToDto).ToList(), request.Page, request.PageSize, total);
    }
}
