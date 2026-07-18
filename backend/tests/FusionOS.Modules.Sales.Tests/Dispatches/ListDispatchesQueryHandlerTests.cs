using FusionOS.Modules.Sales.Application.Dispatches.Contracts;
using FusionOS.Modules.Sales.Application.Dispatches.Queries.ListDispatches;
using FusionOS.Modules.Sales.Domain.Dispatches;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Sales.Tests.Dispatches;

public class ListDispatchesQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsPagedDispatchesForTheCompany()
    {
        var companyId = Guid.NewGuid();
        var dispatch = Dispatch.Create(companyId, Guid.NewGuid(), Guid.NewGuid(), new[] { new DispatchLineInput(Guid.NewGuid(), 3m) });
        var repository = Substitute.For<IDispatchRepository>();
        repository.ListAsync(companyId, 1, 25, Arg.Any<CancellationToken>()).Returns(new[] { dispatch });
        repository.CountAsync(companyId, Arg.Any<CancellationToken>()).Returns(1);
        var handler = new ListDispatchesQueryHandler(repository);

        var result = await handler.Handle(new ListDispatchesQuery(companyId), CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Data.Should().ContainSingle(d => d.Lines.Count == 1);
    }
}
