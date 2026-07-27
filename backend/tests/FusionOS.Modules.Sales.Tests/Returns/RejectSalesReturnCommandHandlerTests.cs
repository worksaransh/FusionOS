using FusionOS.Modules.Sales.Application.Customers.Contracts;
using FusionOS.Modules.Sales.Application.Returns.Commands.RejectSalesReturn;
using FusionOS.Modules.Sales.Application.Returns.Contracts;
using FusionOS.Modules.Sales.Domain.Returns;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Sales.Tests.Returns;

public class RejectSalesReturnCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenRequested_RejectsAndReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var salesReturn = SalesReturn.Create(companyId, Guid.NewGuid(), Guid.NewGuid(), "Damaged goods", new[] { new SalesReturnLineInput(Guid.NewGuid(), 2m) });
        var repository = Substitute.For<ISalesReturnRepository>();
        repository.GetByIdAsync(companyId, salesReturn.Id, Arg.Any<CancellationToken>()).Returns(salesReturn);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new RejectSalesReturnCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(new RejectSalesReturnCommand(companyId, salesReturn.Id, "Outside return window"), CancellationToken.None);

        result.Status.Should().Be(nameof(SalesReturnStatus.Rejected));
        result.RejectionReason.Should().Be("Outside return window");
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenAlreadyApproved_ThrowsInvalidOperationException()
    {
        var companyId = Guid.NewGuid();
        var salesReturn = SalesReturn.Create(companyId, Guid.NewGuid(), Guid.NewGuid(), "Damaged goods", new[] { new SalesReturnLineInput(Guid.NewGuid(), 2m) });
        salesReturn.Approve();
        var repository = Substitute.For<ISalesReturnRepository>();
        repository.GetByIdAsync(companyId, salesReturn.Id, Arg.Any<CancellationToken>()).Returns(salesReturn);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new RejectSalesReturnCommandHandler(repository, unitOfWork);

        var act = () => handler.Handle(new RejectSalesReturnCommand(companyId, salesReturn.Id, "Too late"), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_WhenSalesReturnDoesNotExist_ThrowsKeyNotFoundException()
    {
        var companyId = Guid.NewGuid();
        var salesReturnId = Guid.NewGuid();
        var repository = Substitute.For<ISalesReturnRepository>();
        repository.GetByIdAsync(companyId, salesReturnId, Arg.Any<CancellationToken>()).Returns((SalesReturn?)null);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new RejectSalesReturnCommandHandler(repository, unitOfWork);

        var act = () => handler.Handle(new RejectSalesReturnCommand(companyId, salesReturnId, "Not eligible"), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
