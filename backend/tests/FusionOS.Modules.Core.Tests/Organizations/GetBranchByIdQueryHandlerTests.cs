using FluentAssertions;
using FusionOS.Modules.Core.Application.Branches.Contracts;
using FusionOS.Modules.Core.Application.Branches.Queries.GetBranchById;
using FusionOS.Modules.Core.Domain.Organizations;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Core.Tests.Organizations;

public class GetBranchByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenBranchExists_ReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var branch = Branch.Create(companyId, "Head Office", "HQ-01");
        var repository = Substitute.For<IBranchRepository>();
        repository.GetByIdAsync(companyId, branch.Id, Arg.Any<CancellationToken>()).Returns(branch);
        var handler = new GetBranchByIdQueryHandler(repository);

        var result = await handler.Handle(new GetBranchByIdQuery(companyId, branch.Id), CancellationToken.None);

        result.Code.Should().Be("HQ-01");
    }

    [Fact]
    public async Task Handle_WhenBranchDoesNotExist_ThrowsKeyNotFoundException()
    {
        var companyId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var repository = Substitute.For<IBranchRepository>();
        repository.GetByIdAsync(companyId, branchId, Arg.Any<CancellationToken>()).Returns((Branch?)null);
        var handler = new GetBranchByIdQueryHandler(repository);

        var act = () => handler.Handle(new GetBranchByIdQuery(companyId, branchId), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
