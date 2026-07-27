using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Hrms.Application.Employees.Contracts;
using FusionOS.Modules.Hrms.Domain.Onboarding;
using MediatR;

namespace FusionOS.Modules.Hrms.Application.Offboarding.Queries.ListOffboardingWorkflows;

public sealed class ListOffboardingWorkflowsQueryHandler : IRequestHandler<ListOffboardingWorkflowsQuery, PagedResult<EmployeeWorkflowDto>>
{
    private readonly IEmployeeWorkflowRepository _repository;

    public ListOffboardingWorkflowsQueryHandler(IEmployeeWorkflowRepository repository) => _repository = repository;

    public async Task<PagedResult<EmployeeWorkflowDto>> Handle(ListOffboardingWorkflowsQuery request, CancellationToken cancellationToken)
    {
        var workflows = await _repository.ListAsync(request.CompanyId, WorkflowKind.Offboarding, request.EmployeeId, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, WorkflowKind.Offboarding, request.EmployeeId, cancellationToken);

        return new PagedResult<EmployeeWorkflowDto>(workflows.Select(EmployeeWorkflowMapper.ToDto).ToList(), request.Page, request.PageSize, total);
    }
}
