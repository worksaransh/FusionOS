using FluentAssertions;
using FusionOS.Modules.Core.Application.Branches.Contracts;
using FusionOS.Modules.Core.Application.Branches.Queries.ListBranches;
using FusionOS.Modules.Core.Domain.Organizations;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Core.Tests.Organizations;

public class ListBranchesQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsPagedBranchesForTheCompany()
    {
        var companyId = Guid.NewGuid();
        var branches = new[] { Branch.Create(companyId, "Head Office", "HQ-01") };
        var repository = Substitute.For<IBranchRepository>();
        repository.ListAsync(companyId, null, 1, 25, Arg.Any<CancellationToken>()).Returns(branches);
        repository.CountAsync(companyId, null, Arg.Any<CancellationToken>()).Returns(1);
        var handler = new ListBranchesQueryHandler(repository);

        var result = await handler.Handle(new ListBranchesQuery(companyId), CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Data.Should().ContainSingle(b => b.Code == "HQ-01");
    }
}
