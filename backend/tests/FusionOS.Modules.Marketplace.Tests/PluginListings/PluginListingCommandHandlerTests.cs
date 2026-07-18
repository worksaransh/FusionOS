using FluentAssertions;
using FusionOS.Modules.Marketplace.Application.PluginListings.Commands.CreatePluginListing;
using FusionOS.Modules.Marketplace.Application.PluginListings.Commands.DeactivatePluginListing;
using FusionOS.Modules.Marketplace.Application.PluginListings.Contracts;
using FusionOS.Modules.Marketplace.Domain.PluginListings;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Marketplace.Tests.PluginListings;

public class PluginListingCommandHandlerTests
{
    [Fact]
    public async Task CreatePluginListing_PersistsActiveListing()
    {
        var companyId = Guid.NewGuid();
        var repository = Substitute.For<IPluginListingRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreatePluginListingCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(
            new CreatePluginListingCommand(companyId, "WH-SCAN", "Warehouse Scanner", "Acme Co", PluginCategory.Plugin),
            CancellationToken.None);

        result.Code.Should().Be("WH-SCAN");
        result.IsActive.Should().BeTrue();
        await repository.Received(1).AddAsync(Arg.Any<PluginListing>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeactivatePluginListing_SetsInactive()
    {
        var companyId = Guid.NewGuid();
        var listing = PluginListing.Create(companyId, "WH-SCAN", "Warehouse Scanner", "Acme Co", PluginCategory.Plugin);
        var repository = Substitute.For<IPluginListingRepository>();
        repository.GetByIdAsync(companyId, listing.Id, Arg.Any<CancellationToken>()).Returns(listing);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new DeactivatePluginListingCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(new DeactivatePluginListingCommand(companyId, listing.Id), CancellationToken.None);

        result.IsActive.Should().BeFalse();
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeactivatePluginListing_WhenMissing_ThrowsKeyNotFound()
    {
        var companyId = Guid.NewGuid();
        var repository = Substitute.For<IPluginListingRepository>();
        repository.GetByIdAsync(companyId, Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((PluginListing?)null);
        var handler = new DeactivatePluginListingCommandHandler(repository, Substitute.For<IUnitOfWork>());

        var act = () => handler.Handle(new DeactivatePluginListingCommand(companyId, Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
