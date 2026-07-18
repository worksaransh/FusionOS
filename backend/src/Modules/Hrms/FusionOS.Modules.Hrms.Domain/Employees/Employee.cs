using FusionOS.SharedKernel;
using FusionOS.Modules.Hrms.Domain.Employees.Events;

namespace FusionOS.Modules.Hrms.Domain.Employees;

/// <summary>
/// Phase 4 — HRMS, first slice: employee records (05_MODULE_ROADMAP.md's
/// "Employee records" line item). Pure master data (Code/FullName/Email/
/// DepartmentName/HireDate/IsActive), same shape as Finance's CostCenter.
/// DepartmentName is a plain optional string, not a cross-module reference to
/// Core's Department aggregate — deliberately, same reasoning as Maintenance's
/// Asset.Location: adding that reference now would be scope not asked for.
/// Note this Employee is a distinct, HRMS-owned identity record — it is not
/// the same aggregate as Core's User (which represents "who can log in"); a
/// person can be an Employee without ever being a User, and vice versa
/// (03_SYSTEM_ARCHITECTURE.md §2 — no cross-module foreign key).
/// </summary>
public sealed class Employee : TenantAggregateRoot
{
    public string Code { get; private set; } = default!;
    public string FullName { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string? DepartmentName { get; private set; }
    public DateTimeOffset HireDate { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Employee() { }

    public static Employee Create(Guid companyId, string code, string fullName, string email, string? departmentName, DateTimeOffset hireDate)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Employee code is required.", nameof(code));
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Employee full name is required.", nameof(fullName));
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Employee email is required.", nameof(email));

        var employee = new Employee
        {
            CompanyId = companyId,
            Code = code.Trim().ToUpperInvariant(),
            FullName = fullName.Trim(),
            Email = email.Trim(),
            DepartmentName = string.IsNullOrWhiteSpace(departmentName) ? null : departmentName.Trim(),
            HireDate = hireDate,
        };

        employee.Raise(new EmployeeCreated(employee.Id, companyId, employee.Code));
        return employee;
    }

    public void Deactivate() => IsActive = false;
}
