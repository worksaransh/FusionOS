using FluentAssertions;
using FusionOS.Modules.Core.Application.Companies.Contracts;
using FusionOS.Modules.Core.Application.Departments.Commands.DeactivateDepartment;
using FusionOS.Modules.Core.Application.Departments.Contracts;
using FusionOS.Modules.Core.Domain.Organizations;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Core.Tests.Organizations;

public class DeactivateDepartmentCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenDepartmentExists_DeactivatesAndReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var department = Department.Create(companyId, null, "Engineering", "ENG-01");
        var repository = Substitute.For<IDepartmentRepository>();
        repository.GetByIdAsync(companyId, department.Id, Arg.Any<CancellationToken>()).Returns(department);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new DeactivateDepartmentCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(new DeactivateDepartmentCommand(companyId, department.Id), CancellationToken.None);

        result.IsActive.Should().BeFalse();
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenDepartmentDoesNotExist_ThrowsKeyNotFoundException()
    {
        var companyId = Guid.NewGuid();
        var departmentId = Guid.NewGuid();
        var repository = Substitute.For<IDepartmentRepository>();
        repository.GetByIdAsync(companyId, departmentId, Arg.Any<CancellationToken>()).Returns((Department?)null);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new DeactivateDepartmentCommandHandler(repository, unitOfWork);

        var act = () => handler.Handle(new DeactivateDepartmentCommand(companyId, departmentId), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
