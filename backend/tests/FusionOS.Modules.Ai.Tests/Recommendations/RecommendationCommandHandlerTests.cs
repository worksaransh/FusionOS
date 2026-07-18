using FluentAssertions;
using FusionOS.Modules.Ai.Application.Recommendations.Commands.AcceptRecommendation;
using FusionOS.Modules.Ai.Application.Recommendations.Commands.DismissRecommendation;
using FusionOS.Modules.Ai.Application.Recommendations.Commands.RecordRecommendation;
using FusionOS.Modules.Ai.Application.Recommendations.Contracts;
using FusionOS.Modules.Ai.Domain.Recommendations;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Ai.Tests.Recommendations;

public class RecommendationCommandHandlerTests
{
    [Fact]
    public async Task RecordRecommendation_PersistsPendingRecommendation()
    {
        var companyId = Guid.NewGuid();
        var repository = Substitute.For<IRecommendationRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new RecordRecommendationCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(
            new RecordRecommendationCommand(companyId, "ReorderSuggestion", Guid.NewGuid(), "Reorder 200 units.", "v1.0.0"),
            CancellationToken.None);

        result.Status.Should().Be("Pending");
        await repository.Received(1).AddAsync(Arg.Any<Recommendation>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AcceptRecommendation_ResolvesToAccepted()
    {
        var companyId = Guid.NewGuid();
        var recommendation = Recommendation.Create(companyId, "ReorderSuggestion", Guid.NewGuid(), "Reorder 200 units.", "v1.0.0");
        var repository = Substitute.For<IRecommendationRepository>();
        repository.GetByIdAsync(companyId, recommendation.Id, Arg.Any<CancellationToken>()).Returns(recommendation);
        var handler = new AcceptRecommendationCommandHandler(repository, Substitute.For<IUnitOfWork>());

        var result = await handler.Handle(new AcceptRecommendationCommand(companyId, recommendation.Id), CancellationToken.None);

        result.Status.Should().Be("Accepted");
    }

    [Fact]
    public async Task DismissRecommendation_ResolvesToDismissed()
    {
        var companyId = Guid.NewGuid();
        var recommendation = Recommendation.Create(companyId, "ReorderSuggestion", Guid.NewGuid(), "Reorder 200 units.", "v1.0.0");
        var repository = Substitute.For<IRecommendationRepository>();
        repository.GetByIdAsync(companyId, recommendation.Id, Arg.Any<CancellationToken>()).Returns(recommendation);
        var handler = new DismissRecommendationCommandHandler(repository, Substitute.For<IUnitOfWork>());

        var result = await handler.Handle(new DismissRecommendationCommand(companyId, recommendation.Id), CancellationToken.None);

        result.Status.Should().Be("Dismissed");
    }

    [Fact]
    public async Task AcceptRecommendation_WhenMissing_ThrowsKeyNotFound()
    {
        var companyId = Guid.NewGuid();
        var repository = Substitute.For<IRecommendationRepository>();
        repository.GetByIdAsync(companyId, Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Recommendation?)null);
        var handler = new AcceptRecommendationCommandHandler(repository, Substitute.For<IUnitOfWork>());

        var act = () => handler.Handle(new AcceptRecommendationCommand(companyId, Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
