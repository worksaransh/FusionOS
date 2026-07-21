using FluentAssertions;
using FusionOS.Modules.Core.Application.Branches.Commands.DeactivateBranch;
using FusionOS.Modules.Core.Application.Branches.Contracts;
using FusionOS.Modules.Core.Application.Companies.Contracts;
using FusionOS.Modules.Core.Domain.Organizations;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Core.Tests.Organizations;

public class DeactivateBranchCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenBranchExists_DeactivatesAndReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var branch = Branch.Create(companyId, "Head Office", "HQ-01");
        var repository = Substitute.For<IBranchRepository>();
        repository.GetByIdAsync(companyId, branch.Id, Arg.Any<CancellationToken>()).Returns(branch);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new DeactivateBranchCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(new DeactivateBranchCommand(companyId, branch.Id), CancellationToken.None);

        result.IsActive.Should().BeFalse();
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenBranchDoesNotExist_ThrowsKeyNotFoundException()
    {
        var companyId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var repository = Substitute.For<IBranchRepository>();
        repository.GetByIdAsync(companyId, branchId, Arg.Any<CancellationToken>()).Returns((Branch?)null);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new DeactivateBranchCommandHandler(repository, unitOfWork);

        var act = () => handler.Handle(new DeactivateBranchCommand(companyId, branchId), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
