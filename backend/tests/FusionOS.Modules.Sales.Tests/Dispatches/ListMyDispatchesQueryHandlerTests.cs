using FusionOS.Modules.Sales.Application.Dispatches.Contracts;
using FusionOS.Modules.Sales.Application.Dispatches.Queries.ListMyDispatches;
using FusionOS.Modules.Sales.Domain.Dispatches;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Sales.Tests.Dispatches;

/// <summary>
/// Cross-customer leakage check for the portal - see
/// ListMyQuotationsQueryHandlerTests (Quotations) for the full rationale.
/// Dispatch itself has no CustomerId (only SalesOrderId - see Dispatch's own
/// doc comment); the real Infrastructure implementation joins through
/// SalesOrder to scope by CustomerId (DispatchRepository.ListByCustomerAsync),
/// which is exactly the contract this fake mirrors.
/// </summary>
public class ListMyDispatchesQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsOnlyTheRequestingCustomersOwnDispatches()
    {
        var companyId = Guid.NewGuid();
        var customerA = Guid.NewGuid();
        var customerB = Guid.NewGuid();

        var dispatchForA = Dispatch.Create(companyId, Guid.NewGuid(), Guid.NewGuid(), new[] { new DispatchLineInput(Guid.NewGuid(), 1m) });
        var dispatchForB = Dispatch.Create(companyId, Guid.NewGuid(), Guid.NewGuid(), new[] { new DispatchLineInput(Guid.NewGuid(), 2m) });

        var repository = Substitute.For<IDispatchRepository>();
        repository.ListByCustomerAsync(customerA, 1, 25, Arg.Any<CancellationToken>()).Returns(new[] { dispatchForA });
        repository.CountByCustomerAsync(customerA, Arg.Any<CancellationToken>()).Returns(1);
        repository.ListByCustomerAsync(customerB, 1, 25, Arg.Any<CancellationToken>()).Returns(new[] { dispatchForB });
        repository.CountByCustomerAsync(customerB, Arg.Any<CancellationToken>()).Returns(1);

        var handler = new ListMyDispatchesQueryHandler(repository);

        var resultForA = await handler.Handle(new ListMyDispatchesQuery(customerA), CancellationToken.None);
        var resultForB = await handler.Handle(new ListMyDispatchesQuery(customerB), CancellationToken.None);

        resultForA.Data.Should().ContainSingle(d => d.Id == dispatchForA.Id);
        resultForA.Data.Should().NotContain(d => d.Id == dispatchForB.Id);
        resultForB.Data.Should().ContainSingle(d => d.Id == dispatchForB.Id);
        resultForB.Data.Should().NotContain(d => d.Id == dispatchForA.Id);

        await repository.DidNotReceive().ListAsync(Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }
}
