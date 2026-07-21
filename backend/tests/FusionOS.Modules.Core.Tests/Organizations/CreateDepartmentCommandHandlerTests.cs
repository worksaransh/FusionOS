using FluentAssertions;
using FusionOS.Modules.Core.Application.Companies.Contracts;
using FusionOS.Modules.Core.Application.Departments.Commands.CreateDepartment;
using FusionOS.Modules.Core.Application.Departments.Contracts;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Core.Tests.Organizations;

public class CreateDepartmentCommandHandlerTests
{
    [Fact]
    public async Task Handle_PersistsDepartment()
    {
        var repository = Substitute.For<IDepartmentRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateDepartmentCommandHandler(repository, unitOfWork);
        var companyId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var command = new CreateDepartmentCommand(companyId, branchId, "Engineering", "ENG-01");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Code.Should().Be("ENG-01");
        result.BranchId.Should().Be(branchId);
        await repository.Received(1).AddAsync(Arg.Any<FusionOS.Modules.Core.Domain.Organizations.Department>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
