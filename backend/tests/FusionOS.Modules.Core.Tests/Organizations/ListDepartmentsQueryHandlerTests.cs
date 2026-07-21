using FluentAssertions;
using FusionOS.Modules.Core.Application.Departments.Contracts;
using FusionOS.Modules.Core.Application.Departments.Queries.ListDepartments;
using FusionOS.Modules.Core.Domain.Organizations;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Core.Tests.Organizations;

public class ListDepartmentsQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsPagedDepartmentsForTheCompany()
    {
        var companyId = Guid.NewGuid();
        var departments = new[] { Department.Create(companyId, null, "Engineering", "ENG-01") };
        var repository = Substitute.For<IDepartmentRepository>();
        repository.ListAsync(companyId, null, 1, 25, Arg.Any<CancellationToken>()).Returns(departments);
        repository.CountAsync(companyId, null, Arg.Any<CancellationToken>()).Returns(1);
        var handler = new ListDepartmentsQueryHandler(repository);

        var result = await handler.Handle(new ListDepartmentsQuery(companyId), CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Data.Should().ContainSingle(d => d.Code == "ENG-01");
    }
}
