using FluentAssertions;
using FusionOS.Modules.Core.Application.Branches.Commands.UpdateBranch;
using FusionOS.Modules.Core.Application.Branches.Contracts;
using FusionOS.Modules.Core.Application.Companies.Contracts;
using FusionOS.Modules.Core.Domain.Organizations;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Core.Tests.Organizations;

public class UpdateBranchCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenBranchExists_UpdatesAndReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var branch = Branch.Create(companyId, "Head Office", "HQ-01");
        var repository = Substitute.For<IBranchRepository>();
        repository.GetByIdAsync(companyId, branch.Id, Arg.Any<CancellationToken>()).Returns(branch);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new UpdateBranchCommandHandler(repository, unitOfWork);
        var command = new UpdateBranchCommand(companyId, branch.Id, "Head Office (West)", true);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Name.Should().Be("Head Office (West)");
        result.IsHeadOffice.Should().BeTrue();
        result.Code.Should().Be("HQ-01");
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
        var handler = new UpdateBranchCommandHandler(repository, unitOfWork);
        var command = new UpdateBranchCommand(companyId, branchId, "Head Office", false);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
