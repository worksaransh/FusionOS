using FusionOS.SharedKernel;
using FusionOS.Modules.Hrms.Domain.Employees.Events;

namespace FusionOS.Modules.Hrms.Domain.Employees;

/// <summary>
/// Phase 4 — HRMS, first slice: employee records (05_MODULE_ROADMAP.md's
/// "Employee records" line item). Pure master data (Code/FullName/Email/
/// DepartmentName/HireDate/IsActive), same shape as Finance's CostCenter.
/// Note this Employee is a distinct, HRMS-owned identity record — it is not
/// the same aggregate as Core's User (which represents "who can log in"); a
/// person can be an Employee without ever being a User, and vice versa
/// (03_SYSTEM_ARCHITECTURE.md §2 — no cross-module foreign key).
///
/// Org-chart/Department depth pass (2026-07-23) adds two references:
/// <list type="bullet">
/// <item><see cref="ManagerId"/> — a same-module, opaque reference to another
/// Employee (this aggregate's own type). Existence is validated by the
/// command handler via IEmployeeRepository, same "reference-by-id, no
/// database FK, existence validated in the handler" convention LeaveRequest
/// uses for its EmployeeId. Not validated for cycles at this layer (a full
/// reporting-chain cycle check would need to walk the graph, which is the
/// handler's/a future job, not this aggregate's) — but a same-aggregate-type
/// guard is enforced here: an employee can never be its own manager.</item>
/// <item><see cref="DepartmentId"/> — an opaque <b>cross-module</b> reference
/// into Core's Department aggregate (backend/.../Core.Domain/Organizations/Department.cs).
/// Per 03_SYSTEM_ARCHITECTURE.md's cross-module convention (same as
/// WorkOrder.ProductId/WarehouseId), this is a plain Guid — never a compile-time
/// project reference to Core, never existence-validated here or in the command
/// handler (Hrms's .csproj files have no reference to any Core project, and
/// none was added for this).</item>
/// </list>
/// <see cref="DepartmentName"/> is kept exactly as it was (a plain optional
/// display string) for backward compatibility with existing data and callers
/// — it is now superseded by <see cref="DepartmentId"/> where a real Core
/// Department is linked, but every existing read/write site of DepartmentName
/// (CreateEmployeeCommand, EmployeeDto, the frontend create form) keeps
/// working unchanged. Removing it outright would break those call sites for
/// no functional gain, since nothing requires DepartmentName and DepartmentId
/// to be mutually exclusive — an employee can have a free-text department
/// label, a real Department link, both, or neither.
/// </summary>
public sealed class Employee : TenantAggregateRoot
{
    public string Code { get; private set; } = default!;
    public string FullName { get; private set; } = default!;
    public string Email { get; private set; } = default!;

    /// <summary>Superseded by <see cref="DepartmentId"/> where available — kept for existing data/display and backward compatibility (see class doc comment).</summary>
    public string? DepartmentName { get; private set; }

    public DateTimeOffset HireDate { get; private set; }
    public bool IsActive { get; private set; } = true;

    /// <summary>Opaque cross-module reference to Core's Department aggregate — never existence-validated here (see class doc comment).</summary>
    public Guid? DepartmentId { get; private set; }

    /// <summary>Same-module reference to this employee's manager (another Employee) — existence validated by the command handler, cycles not checked at this layer. See class doc comment.</summary>
    public Guid? ManagerId { get; private set; }

    private Employee() { }

    public static Employee Create(
        Guid companyId,
        string code,
        string fullName,
        string email,
        string? departmentName,
        DateTimeOffset hireDate,
        Guid? departmentId = null,
        Guid? managerId = null)
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
            DepartmentId = departmentId,
        };

        if (managerId.HasValue && managerId.Value == employee.Id)
            throw new ArgumentException("An employee cannot be their own manager.", nameof(managerId));

        employee.ManagerId = managerId;

        employee.Raise(new EmployeeCreated(employee.Id, companyId, employee.Code));
        return employee;
    }

    /// <summary>
    /// Updates the mutable master-data fields (Employee had no Update command at all before this
    /// depth pass — only Create/Deactivate). Code, DepartmentName, and HireDate are deliberately
    /// left out — Code is the tenant-scoped business key (immutable after creation, same
    /// convention as Branch/CostCenter/Company/Department), and DepartmentName/HireDate simply
    /// weren't asked to become editable here; only what the org-chart/Department feature needs
    /// (FullName/Email/DepartmentId/ManagerId) is updatable via this method.
    /// </summary>
    public void UpdateDetails(string fullName, string email, Guid? departmentId, Guid? managerId)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Employee full name is required.", nameof(fullName));
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Employee email is required.", nameof(email));
        if (managerId.HasValue && managerId.Value == Id)
            throw new ArgumentException("An employee cannot be their own manager.", nameof(managerId));

        FullName = fullName.Trim();
        Email = email.Trim();
        DepartmentId = departmentId;
        ManagerId = managerId;
    }

    public void Deactivate() => IsActive = false;
}
