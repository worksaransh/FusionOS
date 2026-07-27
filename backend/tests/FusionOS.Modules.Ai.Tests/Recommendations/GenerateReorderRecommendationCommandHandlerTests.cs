using FluentAssertions;
using FusionOS.Modules.Ai.Application.Forecasting.Contracts;
using FusionOS.Modules.Ai.Application.Forecasting.Queries.GetDemandForecast;
using FusionOS.Modules.Ai.Application.Recommendations.Commands.GenerateReorderRecommendation;
using FusionOS.Modules.Ai.Application.Recommendations.Contracts;
using FusionOS.Modules.Ai.Domain.Recommendations;
using MediatR;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Ai.Tests.Recommendations;

/// <summary>
/// GenerateReorderRecommendationCommandHandler had zero test coverage despite being, per its
/// own doc comment, the "first real producer" that turns a demand forecast into a persisted
/// Recommendation — real branching logic (null on not-forecastable, null on negligible demand,
/// otherwise persist) that's easy to get wrong silently.
/// </summary>
public class GenerateReorderRecommendationCommandHandlerTests
{
    private static GenerateReorderRecommendationCommandHandler MakeHandler(
        ISender sender, out IRecommendationRepository repository, out IUnitOfWork unitOfWork)
    {
        repository = Substitute.For<IRecommendationRepository>();
        unitOfWork = Substitute.For<IUnitOfWork>();
        return new GenerateReorderRecommendationCommandHandler(sender, repository, unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenNotForecastable_ReturnsNullWithoutPersisting()
    {
        var companyId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var sender = Substitute.For<ISender>();
        sender.Send(Arg.Any<GetDemandForecastQuery>(), Arg.Any<CancellationToken>())
            .Returns(new DemandForecastDto(productId, IsForecastable: false, "Insufficient history", null, null, Array.Empty<ForecastPointDto>()));
        var handler = MakeHandler(sender, out var repository, out var unitOfWork);

        var result = await handler.Handle(new GenerateReorderRecommendationCommand(companyId, productId), CancellationToken.None);

        result.Should().BeNull();
        await repository.DidNotReceive().AddAsync(Arg.Any<Recommendation>(), Arg.Any<CancellationToken>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenForecastableButDemandNegligible_ReturnsNullWithoutPersisting()
    {
        var companyId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var sender = Substitute.For<ISender>();
        // Both figures below the 0.01 MeaningfulDailyDemandThreshold — nothing worth reordering for.
        sender.Send(Arg.Any<GetDemandForecastQuery>(), Arg.Any<CancellationToken>())
            .Returns(new DemandForecastDto(productId, IsForecastable: true, null, 0.005m, 0.004m, Array.Empty<ForecastPointDto>()));
        var handler = MakeHandler(sender, out var repository, out var unitOfWork);

        var result = await handler.Handle(new GenerateReorderRecommendationCommand(companyId, productId), CancellationToken.None);

        result.Should().BeNull();
        await repository.DidNotReceive().AddAsync(Arg.Any<Recommendation>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTrendMeetsThresholdButMovingAverageDoesNot_StillPersists()
    {
        // The guard is "both below threshold" (&&) — a forecast where only ONE of the two
        // figures is negligible must still produce a recommendation.
        var companyId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var sender = Substitute.For<ISender>();
        sender.Send(Arg.Any<GetDemandForecastQuery>(), Arg.Any<CancellationToken>())
            .Returns(new DemandForecastDto(productId, IsForecastable: true, null, 5m, 0.001m, Array.Empty<ForecastPointDto>()));
        var handler = MakeHandler(sender, out var repository, out var unitOfWork);

        var result = await handler.Handle(new GenerateReorderRecommendationCommand(companyId, productId), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Type.Should().Be("ReorderSuggestion");
        result.ModelVersion.Should().Be("linear-regression-v1");
        await repository.Received(1).AddAsync(Arg.Any<Recommendation>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenForecastableWithMeaningfulDemand_PersistsRecommendationReferencingProduct()
    {
        var companyId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var sender = Substitute.For<ISender>();
        sender.Send(Arg.Any<GetDemandForecastQuery>(), Arg.Any<CancellationToken>())
            .Returns(new DemandForecastDto(productId, IsForecastable: true, null, 12.5m, 10.2m, Array.Empty<ForecastPointDto>()));
        var handler = MakeHandler(sender, out var repository, out var unitOfWork);

        var result = await handler.Handle(new GenerateReorderRecommendationCommand(companyId, productId), CancellationToken.None);

        result.Should().NotBeNull();
        result!.ReferenceId.Should().Be(productId);
        result.Status.Should().Be("Pending");
        result.Summary.Should().Contain("12.5").And.Contain("10.2");
    }
}
