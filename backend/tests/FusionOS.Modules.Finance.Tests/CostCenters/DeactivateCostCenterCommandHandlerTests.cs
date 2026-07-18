using FusionOS.Modules.Finance.Application.CostCenters.Commands.DeactivateCostCenter;
using FusionOS.Modules.Finance.Application.CostCenters.Contracts;
using FusionOS.Modules.Finance.Domain.CostCenters;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.CostCenters;

public class DeactivateCostCenterCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenCostCenterExists_DeactivatesAndReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var costCenter = CostCenter.Create(companyId, "CC-100", "Manufacturing");
        var repository = Substitute.For<ICostCenterRepository>();
        repository.GetByIdAsync(companyId, costCenter.Id, Arg.Any<CancellationToken>()).Returns(costCenter);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new DeactivateCostCenterCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(new DeactivateCostCenterCommand(companyId, costCenter.Id), CancellationToken.None);

        result.IsActive.Should().BeFalse();
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
        var handler = new DeactivateCostCenterCommandHandler(repository, unitOfWork);

        var act = () => handler.Handle(new DeactivateCostCenterCommand(companyId, costCenterId), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
