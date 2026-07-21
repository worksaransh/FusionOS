using FluentAssertions;
using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Core.Application.Branches.Commands.CreateBranch;
using FusionOS.Modules.Core.Application.Branches.Contracts;
using FusionOS.Modules.Core.Application.Companies.Contracts;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Core.Tests.Organizations;

public class CreateBranchCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenCodeIsNew_PersistsBranch()
    {
        var repository = Substitute.For<IBranchRepository>();
        repository.CodeExistsAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateBranchCommandHandler(repository, unitOfWork);
        var command = new CreateBranchCommand(Guid.NewGuid(), "Head Office", "HQ-01");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Code.Should().Be("HQ-01");
        await repository.Received(1).AddAsync(Arg.Any<FusionOS.Modules.Core.Domain.Organizations.Branch>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCodeAlreadyExists_Throws()
    {
        var repository = Substitute.For<IBranchRepository>();
        repository.CodeExistsAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(true);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateBranchCommandHandler(repository, unitOfWork);
        var command = new CreateBranchCommand(Guid.NewGuid(), "Head Office", "HQ-01");

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
