namespace FusionOS.Modules.Hrms.Application.Employees.Contracts;

/// <summary>
/// A single-level "my reports" org-chart view: one employee plus their direct reports (other
/// employees whose ManagerId points at them). Deliberately NOT a full recursive
/// reports-of-reports tree — that would need either a recursive CTE or in-memory graph walk, and
/// nothing in this pass asked for more than "who reports to this person." A full recursive tree
/// is a reasonable future follow-up (see GetEmployeeOrgChartQueryHandler's own doc comment).
/// </summary>
public sealed record EmployeeOrgChartDto(EmployeeDto Employee, IReadOnlyList<EmployeeDto> DirectReports);
