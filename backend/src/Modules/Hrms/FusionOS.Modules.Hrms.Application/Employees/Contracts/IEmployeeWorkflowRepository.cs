using FusionOS.Modules.Hrms.Domain.Onboarding;

namespace FusionOS.Modules.Hrms.Application.Employees.Contracts;

/// <summary>Shared by both Onboarding/* and Offboarding/* command/query handlers — one repository for the one shared EmployeeWorkflow entity (see EmployeeWorkflow.cs's own doc comment). Every method takes/filters by <see cref="WorkflowKind"/> so onboarding handlers can never see an offboarding workflow and vice versa.</summary>
public interface IEmployeeWorkflowRepository
{
    Task<EmployeeWorkflow?> GetByIdAsync(Guid companyId, Guid workflowId, WorkflowKind kind, CancellationToken cancellationToken = default);
    Task AddAsync(EmployeeWorkflow workflow, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EmployeeWorkflow>> ListAsync(Guid companyId, WorkflowKind kind, Guid? employeeId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, WorkflowKind kind, Guid? employeeId, CancellationToken cancellationToken = default);
}
