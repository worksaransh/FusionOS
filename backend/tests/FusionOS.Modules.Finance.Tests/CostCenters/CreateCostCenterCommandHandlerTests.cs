using FluentAssertions;
using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Finance.Application.CostCenters.Commands.CreateCostCenter;
using FusionOS.Modules.Finance.Application.CostCenters.Contracts;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.CostCenters;

public class CreateCostCenterCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenCodeIsNew_PersistsCostCenter()
    {
        var repository = Substitute.For<ICostCenterRepository>();
        repository.CodeExistsAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateCostCenterCommandHandler(repository, unitOfWork);
        var command = new CreateCostCenterCommand(Guid.NewGuid(), "CC-100", "Manufacturing");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Code.Should().Be("CC-100");
        await repository.Received(1).AddAsync(Arg.Any<FusionOS.Modules.Finance.Domain.CostCenters.CostCenter>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCodeAlreadyExists_Throws()
    {
        var repository = Substitute.For<ICostCenterRepository>();
        repository.CodeExistsAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(true);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateCostCenterCommandHandler(repository, unitOfWork);
        var command = new CreateCostCenterCommand(Guid.NewGuid(), "CC-100", "Manufacturing");

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
