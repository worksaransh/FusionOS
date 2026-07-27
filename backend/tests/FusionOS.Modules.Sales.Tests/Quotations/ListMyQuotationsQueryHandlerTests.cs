using FusionOS.Modules.Sales.Application.Quotations.Contracts;
using FusionOS.Modules.Sales.Application.Quotations.Queries.ListMyQuotations;
using FusionOS.Modules.Sales.Domain.Quotations;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Sales.Tests.Quotations;

/// <summary>
/// The single most important security property of the Customer Portal: a
/// customer must never see another customer's rows. This drives the query
/// straight through IQuotationRepository.ListByCustomerAsync/CountByCustomerAsync
/// - the same repository methods a real EF-backed implementation filters by
/// CustomerId - with two different customers' quotations present, and asserts
/// zero cross-customer leakage in either direction.
/// </summary>
public class ListMyQuotationsQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsOnlyTheRequestingCustomersOwnQuotations()
    {
        var companyId = Guid.NewGuid();
        var customerA = Guid.NewGuid();
        var customerB = Guid.NewGuid();

        var quotationForA = Quotation.Create(companyId, customerA, new[] { new QuotationLineInput(Guid.NewGuid(), 1m, 100m) });
        var quotationForB = Quotation.Create(companyId, customerB, new[] { new QuotationLineInput(Guid.NewGuid(), 1m, 200m) });

        var repository = Substitute.For<IQuotationRepository>();
        // A real repository implementation filters server-side by CustomerId;
        // this fake mirrors that contract exactly rather than filtering an
        // in-memory list of "everything", so the test actually exercises the
        // scoping the handler is required to request.
        repository.ListByCustomerAsync(customerA, 1, 25, Arg.Any<CancellationToken>())
            .Returns(new[] { quotationForA });
        repository.CountByCustomerAsync(customerA, Arg.Any<CancellationToken>()).Returns(1);
        repository.ListByCustomerAsync(customerB, 1, 25, Arg.Any<CancellationToken>())
            .Returns(new[] { quotationForB });
        repository.CountByCustomerAsync(customerB, Arg.Any<CancellationToken>()).Returns(1);

        var handler = new ListMyQuotationsQueryHandler(repository);

        var resultForA = await handler.Handle(new ListMyQuotationsQuery(customerA), CancellationToken.None);
        var resultForB = await handler.Handle(new ListMyQuotationsQuery(customerB), CancellationToken.None);

        resultForA.Data.Should().ContainSingle(q => q.Id == quotationForA.Id);
        resultForA.Data.Should().NotContain(q => q.Id == quotationForB.Id);
        resultForA.TotalCount.Should().Be(1);

        resultForB.Data.Should().ContainSingle(q => q.Id == quotationForB.Id);
        resultForB.Data.Should().NotContain(q => q.Id == quotationForA.Id);
        resultForB.TotalCount.Should().Be(1);

        // The handler must ask the repository for exactly the requesting
        // customer's id - never CompanyId, never the other customer's id.
        await repository.Received(1).ListByCustomerAsync(customerA, 1, 25, Arg.Any<CancellationToken>());
        await repository.Received(1).ListByCustomerAsync(customerB, 1, 25, Arg.Any<CancellationToken>());
        await repository.DidNotReceive().ListAsync(Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }
}
