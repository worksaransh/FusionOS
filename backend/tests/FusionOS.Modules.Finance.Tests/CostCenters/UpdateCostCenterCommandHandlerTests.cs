using FusionOS.Modules.Finance.Application.CostCenters.Commands.UpdateCostCenter;
using FusionOS.Modules.Finance.Application.CostCenters.Contracts;
using FusionOS.Modules.Finance.Domain.CostCenters;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.CostCenters;

public class UpdateCostCenterCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenCostCenterExists_UpdatesAndReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var costCenter = CostCenter.Create(companyId, "CC-100", "Manufacturing");
        var repository = Substitute.For<ICostCenterRepository>();
        repository.GetByIdAsync(companyId, costCenter.Id, Arg.Any<CancellationToken>()).Returns(costCenter);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new UpdateCostCenterCommandHandler(repository, unitOfWork);
        var command = new UpdateCostCenterCommand(companyId, costCenter.Id, "Manufacturing (West)");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Name.Should().Be("Manufacturing (West)");
        result.Code.Should().Be("CC-100");
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCostCenterDoesNotExist_ThrowsKeyNotFoundException()
    {
        var companyId = Guid.NewGuid();
        var costCenterId = Guid.NewGuid();
        var repository = Substitute.For<ICostCenterRepository>();
        repository.GetByIdAsync(companyId, costCenterId, Arg.Any<CancellationToken>()).Returns((CostCenter?)null);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new UpdateCostCenterCommandHandler(repository, unitOfWork);
        var command = new UpdateCostCenterCommand(companyId, costCenterId, "Manufacturing");

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
