using FluentAssertions;
using FusionOS.Modules.Crm.Application.Activities.Commands.CreateActivity;
using FusionOS.Modules.Crm.Application.Activities.Contracts;
using FusionOS.Modules.Crm.Application.Leads.Contracts;
using FusionOS.Modules.Crm.Domain.Activities;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Crm.Tests.Activities;

public class ActivityCommandHandlerTests
{
    [Fact]
    public async Task CreateActivity_Persists()
    {
        var companyId = Guid.NewGuid();
        var entityId = Guid.NewGuid();
        var repository = Substitute.For<IActivityRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateActivityCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(
            new CreateActivityCommand(companyId, "Opportunity", entityId, "Meeting", "Discussed contract terms."), CancellationToken.None);

        result.EntityType.Should().Be("Opportunity");
        result.EntityId.Should().Be(entityId);
        result.Type.Should().Be("Meeting");
        await repository.Received(1).AddAsync(Arg.Any<Activity>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
