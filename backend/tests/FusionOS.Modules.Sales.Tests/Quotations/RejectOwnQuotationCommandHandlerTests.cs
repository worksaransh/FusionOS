using FusionOS.Modules.Sales.Application.Customers.Contracts;
using FusionOS.Modules.Sales.Application.Quotations.Commands.RejectOwnQuotation;
using FusionOS.Modules.Sales.Application.Quotations.Contracts;
using FusionOS.Modules.Sales.Domain.Quotations;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Sales.Tests.Quotations;

public class RejectOwnQuotationCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenQuotationBelongsToCaller_RejectsAndReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var quotation = Quotation.Create(companyId, customerId, new[] { new QuotationLineInput(Guid.NewGuid(), 2m, 50m) });

        var repository = Substitute.For<IQuotationRepository>();
        repository.GetByIdForCustomerAsync(customerId, quotation.Id, Arg.Any<CancellationToken>()).Returns(quotation);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new RejectOwnQuotationCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(new RejectOwnQuotationCommand(customerId, quotation.Id), CancellationToken.None);

        result.Status.Should().Be(nameof(QuotationStatus.Rejected));
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenQuotationBelongsToAnotherCustomer_ThrowsKeyNotFoundException()
    {
        var callerCustomerId = Guid.NewGuid();
        var quotationId = Guid.NewGuid();

        var repository = Substitute.For<IQuotationRepository>();
        repository.GetByIdForCustomerAsync(callerCustomerId, quotationId, Arg.Any<CancellationToken>()).Returns((Quotation?)null);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new RejectOwnQuotationCommandHandler(repository, unitOfWork);

        var act = () => handler.Handle(new RejectOwnQuotationCommand(callerCustomerId, quotationId), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
