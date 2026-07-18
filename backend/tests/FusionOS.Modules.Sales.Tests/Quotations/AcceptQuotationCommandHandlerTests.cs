using FusionOS.Modules.Sales.Application.Quotations.Commands.AcceptQuotation;
using FusionOS.Modules.Sales.Application.Quotations.Contracts;
using FusionOS.Modules.Sales.Domain.Quotations;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Sales.Tests.Quotations;

public class AcceptQuotationCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenQuotationIsDraft_AcceptsAndReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var quotation = Quotation.Create(companyId, Guid.NewGuid(), new[] { new QuotationLineInput(Guid.NewGuid(), 2m, 50m) });
        var repository = Substitute.For<IQuotationRepository>();
        repository.GetByIdAsync(companyId, quotation.Id, Arg.Any<CancellationToken>()).Returns(quotation);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new AcceptQuotationCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(new AcceptQuotationCommand(companyId, quotation.Id), CancellationToken.None);

        result.Status.Should().Be(nameof(QuotationStatus.Accepted));
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenQuotationIsAlreadyAccepted_ThrowsInvalidOperationException()
    {
        var companyId = Guid.NewGuid();
        var quotation = Quotation.Create(companyId, Guid.NewGuid(), new[] { new QuotationLineInput(Guid.NewGuid(), 2m, 50m) });
        var repository = Substitute.For<IQuotationRepository>();
        repository.GetByIdAsync(companyId, quotation.Id, Arg.Any<CancellationToken>()).Returns(quotation);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new AcceptQuotationCommandHandler(repository, unitOfWork);
        await handler.Handle(new AcceptQuotationCommand(companyId, quotation.Id), CancellationToken.None);

        var act = () => handler.Handle(new AcceptQuotationCommand(companyId, quotation.Id), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_WhenQuotationDoesNotExist_ThrowsKeyNotFoundException()
    {
        var companyId = Guid.NewGuid();
        var quotationId = Guid.NewGuid();
        var repository = Substitute.For<IQuotationRepository>();
        repository.GetByIdAsync(companyId, quotationId, Arg.Any<CancellationToken>()).Returns((Quotation?)null);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new AcceptQuotationCommandHandler(repository, unitOfWork);

        var act = () => handler.Handle(new AcceptQuotationCommand(companyId, quotationId), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
