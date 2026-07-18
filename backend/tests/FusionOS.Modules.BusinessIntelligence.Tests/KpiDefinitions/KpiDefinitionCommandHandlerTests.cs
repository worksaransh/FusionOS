using FluentAssertions;
using FusionOS.Modules.BusinessIntelligence.Application.KpiDefinitions.Commands.CreateKpiDefinition;
using FusionOS.Modules.BusinessIntelligence.Application.KpiDefinitions.Commands.DeactivateKpiDefinition;
using FusionOS.Modules.BusinessIntelligence.Application.KpiDefinitions.Contracts;
using FusionOS.Modules.BusinessIntelligence.Domain.KpiDefinitions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.BusinessIntelligence.Tests.KpiDefinitions;

public class KpiDefinitionCommandHandlerTests
{
    [Fact]
    public async Task CreateKpiDefinition_PersistsActiveKpi()
    {
        var companyId = Guid.NewGuid();
        var repository = Substitute.For<IKpiDefinitionRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateKpiDefinitionCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(new CreateKpiDefinitionCommand(companyId, "OTD", "On-Time Delivery", "%"), CancellationToken.None);

        result.Code.Should().Be("OTD");
        result.IsActive.Should().BeTrue();
        await repository.Received(1).AddAsync(Arg.Any<KpiDefinition>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeactivateKpiDefinition_SetsInactive()
    {
        var companyId = Guid.NewGuid();
        var kpi = KpiDefinition.Create(companyId, "OTD", "On-Time Delivery", null);
        var repository = Substitute.For<IKpiDefinitionRepository>();
        repository.GetByIdAsync(companyId, kpi.Id, Arg.Any<CancellationToken>()).Returns(kpi);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new DeactivateKpiDefinitionCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(new DeactivateKpiDefinitionCommand(companyId, kpi.Id), CancellationToken.None);

        result.IsActive.Should().BeFalse();
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeactivateKpiDefinition_WhenMissing_ThrowsKeyNotFound()
    {
        var companyId = Guid.NewGuid();
        var repository = Substitute.For<IKpiDefinitionRepository>();
        repository.GetByIdAsync(companyId, Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((KpiDefinition?)null);
        var handler = new DeactivateKpiDefinitionCommandHandler(repository, Substitute.For<IUnitOfWork>());

        var act = () => handler.Handle(new DeactivateKpiDefinitionCommand(companyId, Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
