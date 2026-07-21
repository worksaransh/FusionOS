using FluentAssertions;
using FusionOS.Modules.Core.Application.Companies.Contracts;
using FusionOS.Modules.Core.Application.Departments.Commands.UpdateDepartment;
using FusionOS.Modules.Core.Application.Departments.Contracts;
using FusionOS.Modules.Core.Domain.Organizations;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Core.Tests.Organizations;

public class UpdateDepartmentCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenDepartmentExists_UpdatesAndReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var department = Department.Create(companyId, null, "Engineering", "ENG-01");
        var repository = Substitute.For<IDepartmentRepository>();
        repository.GetByIdAsync(companyId, department.Id, Arg.Any<CancellationToken>()).Returns(department);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new UpdateDepartmentCommandHandler(repository, unitOfWork);
        var newBranchId = Guid.NewGuid();
        var command = new UpdateDepartmentCommand(companyId, department.Id, "Engineering (West)", newBranchId, null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Name.Should().Be("Engineering (West)");
        result.BranchId.Should().Be(newBranchId);
        result.Code.Should().Be("ENG-01");
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
        var handler = new UpdateDepartmentCommandHandler(repository, unitOfWork);
        var command = new UpdateDepartmentCommand(companyId, departmentId, "Engineering", null, null);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
