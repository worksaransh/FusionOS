using FluentAssertions;
using FusionOS.Modules.Core.Application.Departments.Contracts;
using FusionOS.Modules.Core.Application.Departments.Queries.GetDepartmentById;
using FusionOS.Modules.Core.Domain.Organizations;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Core.Tests.Organizations;

public class GetDepartmentByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenDepartmentExists_ReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var department = Department.Create(companyId, null, "Engineering", "ENG-01");
        var repository = Substitute.For<IDepartmentRepository>();
        repository.GetByIdAsync(companyId, department.Id, Arg.Any<CancellationToken>()).Returns(department);
        var handler = new GetDepartmentByIdQueryHandler(repository);

        var result = await handler.Handle(new GetDepartmentByIdQuery(companyId, department.Id), CancellationToken.None);

        result.Code.Should().Be("ENG-01");
    }

    [Fact]
    public async Task Handle_WhenDepartmentDoesNotExist_ThrowsKeyNotFoundException()
    {
        var companyId = Guid.NewGuid();
        var departmentId = Guid.NewGuid();
        var repository = Substitute.For<IDepartmentRepository>();
        repository.GetByIdAsync(companyId, departmentId, Arg.Any<CancellationToken>()).Returns((Department?)null);
        var handler = new GetDepartmentByIdQueryHandler(repository);

        var act = () => handler.Handle(new GetDepartmentByIdQuery(companyId, departmentId), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
