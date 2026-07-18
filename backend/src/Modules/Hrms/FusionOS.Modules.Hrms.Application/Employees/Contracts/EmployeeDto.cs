namespace FusionOS.Modules.Hrms.Application.Employees.Contracts;

public sealed record EmployeeDto(Guid Id, string Code, string FullName, string Email, string? DepartmentName, DateTimeOffset HireDate, bool IsActive);

/// <summary>Single place that turns an Employee aggregate into its DTO, shared by every handler that returns one.</summary>
public static class EmployeeMapper
{
    public static EmployeeDto ToDto(Domain.Employees.Employee employee) =>
        new(employee.Id, employee.Code, employee.FullName, employee.Email, employee.DepartmentName, employee.HireDate, employee.IsActive);
}
