using FluentAssertions;
using FusionOS.Modules.Hrms.Application.Employees.Commands.CreateEmployee;
using FusionOS.Modules.Hrms.Application.Employees.Commands.DeactivateEmployee;
using FusionOS.Modules.Hrms.Application.Employees.Contracts;
using FusionOS.Modules.Hrms.Domain.Employees;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Hrms.Tests.Employees;

public class EmployeeCommandHandlerTests
{
    private static readonly DateTimeOffset HireDate = new(2024, 1, 15, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task CreateEmployee_PersistsActiveEmployee()
    {
        var companyId = Guid.NewGuid();
        var repository = Substitute.For<IEmployeeRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateEmployeeCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(
            new CreateEmployeeCommand(companyId, "EMP-01", "Jane Doe", "jane@example.com", "Engineering", HireDate),
            CancellationToken.None);

        result.Code.Should().Be("EMP-01");
        result.IsActive.Should().BeTrue();
        await repository.Received(1).AddAsync(Arg.Any<Employee>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeactivateEmployee_SetsInactive()
    {
        var companyId = Guid.NewGuid();
        var employee = Employee.Create(companyId, "EMP-01", "Jane Doe", "jane@example.com", null, HireDate);
        var repository = Substitute.For<IEmployeeRepository>();
        repository.GetByIdAsync(companyId, employee.Id, Arg.Any<CancellationToken>()).Returns(employee);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new DeactivateEmployeeCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(new DeactivateEmployeeCommand(companyId, employee.Id), CancellationToken.None);

        result.IsActive.Should().BeFalse();
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeactivateEmployee_WhenMissing_ThrowsKeyNotFound()
    {
        var companyId = Guid.NewGuid();
        var repository = Substitute.For<IEmployeeRepository>();
        repository.GetByIdAsync(companyId, Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Employee?)null);
        var handler = new DeactivateEmployeeCommandHandler(repository, Substitute.For<IUnitOfWork>());

        var act = () => handler.Handle(new DeactivateEmployeeCommand(companyId, Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
