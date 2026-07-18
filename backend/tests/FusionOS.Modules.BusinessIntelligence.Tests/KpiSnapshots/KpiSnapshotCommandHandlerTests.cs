using FluentAssertions;
using FusionOS.Modules.BusinessIntelligence.Application.KpiDefinitions.Contracts;
using FusionOS.Modules.BusinessIntelligence.Application.KpiSnapshots.Commands.RecordKpiSnapshot;
using FusionOS.Modules.BusinessIntelligence.Application.KpiSnapshots.Contracts;
using FusionOS.Modules.BusinessIntelligence.Domain.KpiSnapshots;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.BusinessIntelligence.Tests.KpiSnapshots;

public class KpiSnapshotCommandHandlerTests
{
    [Fact]
    public async Task RecordKpiSnapshot_WhenKpiDefinitionExists_PersistsSnapshot()
    {
        var companyId = Guid.NewGuid();
        var kpiDefinitionId = Guid.NewGuid();
        var repository = Substitute.For<IKpiSnapshotRepository>();
        var kpiDefinitionRepository = Substitute.For<IKpiDefinitionRepository>();
        kpiDefinitionRepository.ExistsAsync(companyId, kpiDefinitionId, Arg.Any<CancellationToken>()).Returns(true);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new RecordKpiSnapshotCommandHandler(repository, kpiDefinitionRepository, unitOfWork);

        var result = await handler.Handle(new RecordKpiSnapshotCommand(companyId, kpiDefinitionId, 97.5m, "Week 28"), CancellationToken.None);

        result.Value.Should().Be(97.5m);
        await repository.Received(1).AddAsync(Arg.Any<KpiSnapshot>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RecordKpiSnapshot_WhenKpiDefinitionMissing_ThrowsValidation()
    {
        var companyId = Guid.NewGuid();
        var repository = Substitute.For<IKpiSnapshotRepository>();
        var kpiDefinitionRepository = Substitute.For<IKpiDefinitionRepository>();
        kpiDefinitionRepository.ExistsAsync(companyId, Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        var handler = new RecordKpiSnapshotCommandHandler(repository, kpiDefinitionRepository, Substitute.For<IUnitOfWork>());

        var act = () => handler.Handle(new RecordKpiSnapshotCommand(companyId, Guid.NewGuid(), 97.5m, null), CancellationToken.None);

        await act.Should().ThrowAsync<FusionOS.BuildingBlocks.Application.Exceptions.ValidationException>();
    }
}
