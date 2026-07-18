using FluentAssertions;
using FusionOS.Modules.Hrms.Domain.Employees;
using FusionOS.Modules.Hrms.Domain.Employees.Events;
using Xunit;

namespace FusionOS.Modules.Hrms.Tests.Employees;

public class EmployeeTests
{
    private static readonly Guid Company = Guid.NewGuid();
    private static readonly DateTimeOffset HireDate = new(2024, 1, 15, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_WithValidFields_RaisesCreatedEvent()
    {
        var employee = Employee.Create(Company, "emp-01", "Jane Doe", "jane@example.com", "Engineering", HireDate);

        employee.Code.Should().Be("EMP-01");
        employee.FullName.Should().Be("Jane Doe");
        employee.DepartmentName.Should().Be("Engineering");
        employee.IsActive.Should().BeTrue();
        employee.DomainEvents.Should().ContainSingle(e => e is EmployeeCreated);
    }

    [Fact]
    public void Create_WithNoDepartment_LeavesDepartmentNull()
    {
        var employee = Employee.Create(Company, "EMP-02", "John Smith", "john@example.com", null, HireDate);

        employee.DepartmentName.Should().BeNull();
    }

    [Fact]
    public void Create_WithBlankEmail_Throws()
    {
        var act = () => Employee.Create(Company, "EMP-01", "Jane Doe", "  ", null, HireDate);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        var employee = Employee.Create(Company, "EMP-01", "Jane Doe", "jane@example.com", null, HireDate);

        employee.Deactivate();

        employee.IsActive.Should().BeFalse();
    }
}
