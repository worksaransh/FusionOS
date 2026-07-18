using FusionOS.Modules.Sales.Application.PriceLists.Commands.CreatePriceList;
using FusionOS.Modules.Sales.Application.PriceLists.Contracts;
using FusionOS.Modules.Sales.Domain.PriceLists;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Sales.Tests.PriceLists;

public class CreatePriceListCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidEntries_PersistsPriceList()
    {
        var repository = Substitute.For<IPriceListRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreatePriceListCommandHandler(repository, unitOfWork);
        var command = new CreatePriceListCommand(Guid.NewGuid(), "Wholesale", new[] { new PriceListEntryInput(Guid.NewGuid(), 42m) });

        var result = await handler.Handle(command, CancellationToken.None);

        result.Name.Should().Be("Wholesale");
        result.Entries.Should().ContainSingle();
        await repository.Received(1).AddAsync(Arg.Any<PriceList>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
