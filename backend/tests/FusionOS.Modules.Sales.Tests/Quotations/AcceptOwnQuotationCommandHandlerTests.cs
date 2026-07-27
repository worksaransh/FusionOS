using FusionOS.Modules.Sales.Application.Customers.Contracts;
using FusionOS.Modules.Sales.Application.Quotations.Commands.AcceptOwnQuotation;
using FusionOS.Modules.Sales.Application.Quotations.Contracts;
using FusionOS.Modules.Sales.Domain.Quotations;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Sales.Tests.Quotations;

public class AcceptOwnQuotationCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenQuotationBelongsToCaller_AcceptsAndReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var quotation = Quotation.Create(companyId, customerId, new[] { new QuotationLineInput(Guid.NewGuid(), 2m, 50m) });

        var repository = Substitute.For<IQuotationRepository>();
        repository.GetByIdForCustomerAsync(customerId, quotation.Id, Arg.Any<CancellationToken>()).Returns(quotation);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new AcceptOwnQuotationCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(new AcceptOwnQuotationCommand(customerId, quotation.Id), CancellationToken.None);

        result.Status.Should().Be(nameof(QuotationStatus.Accepted));
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// A different customer's quotation id must come back as "not found", not
    /// "forbidden" and never actually loaded/mutated - this IS the portal's
    /// ownership check (see AcceptOwnQuotationCommand's doc comment): the
    /// repository call is scoped by the CALLER's own CustomerId, so another
    /// customer's quotation simply never matches and GetByIdForCustomerAsync
    /// returns null exactly as it would for a genuinely unknown id.
    /// </summary>
    [Fact]
    public async Task Handle_WhenQuotationBelongsToAnotherCustomer_ThrowsKeyNotFoundException()
    {
        var callerCustomerId = Guid.NewGuid();
        var quotationId = Guid.NewGuid();

        var repository = Substitute.For<IQuotationRepository>();
        repository.GetByIdForCustomerAsync(callerCustomerId, quotationId, Arg.Any<CancellationToken>()).Returns((Quotation?)null);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new AcceptOwnQuotationCommandHandler(repository, unitOfWork);

        var act = () => handler.Handle(new AcceptOwnQuotationCommand(callerCustomerId, quotationId), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
