using FusionOS.Modules.Sales.Application.Customers.Contracts;
using FusionOS.Modules.Sales.Application.Returns.Commands.CompleteSalesReturn;
using FusionOS.Modules.Sales.Application.Returns.Contracts;
using FusionOS.Modules.Sales.Domain.Returns;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Sales.Tests.Returns;

public class CompleteSalesReturnCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenApproved_CompletesAndAppliesLineCompletions()
    {
        var companyId = Guid.NewGuid();
        var salesReturn = SalesReturn.Create(companyId, Guid.NewGuid(), Guid.NewGuid(), "Damaged goods", new[] { new SalesReturnLineInput(Guid.NewGuid(), 2m) });
        salesReturn.Approve();
        var lineId = salesReturn.Lines[0].Id;
        var repository = Substitute.For<ISalesReturnRepository>();
        repository.GetByIdAsync(companyId, salesReturn.Id, Arg.Any<CancellationToken>()).Returns(salesReturn);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CompleteSalesReturnCommandHandler(repository, unitOfWork);
        var command = new CompleteSalesReturnCommand(companyId, salesReturn.Id, new[] { new SalesReturnLineCompletion(lineId, true, "Unopened, resellable") });

        var result = await handler.Handle(command, CancellationToken.None);

        result.Status.Should().Be(nameof(SalesReturnStatus.Completed));
        result.Lines[0].Restocked.Should().BeTrue();
        result.Lines[0].ConditionNotes.Should().Be("Unopened, resellable");
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenStillRequested_ThrowsInvalidOperationException()
    {
        var companyId = Guid.NewGuid();
        var salesReturn = SalesReturn.Create(companyId, Guid.NewGuid(), Guid.NewGuid(), "Damaged goods", new[] { new SalesReturnLineInput(Guid.NewGuid(), 2m) });
        var lineId = salesReturn.Lines[0].Id;
        var repository = Substitute.For<ISalesReturnRepository>();
        repository.GetByIdAsync(companyId, salesReturn.Id, Arg.Any<CancellationToken>()).Returns(salesReturn);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CompleteSalesReturnCommandHandler(repository, unitOfWork);
        var command = new CompleteSalesReturnCommand(companyId, salesReturn.Id, new[] { new SalesReturnLineCompletion(lineId, true, null) });

        var act = () => handler.Handle(command, CancellationToken.None);

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
        var handler = new CompleteSalesReturnCommandHandler(repository, unitOfWork);
        var command = new CompleteSalesReturnCommand(companyId, salesReturnId, new[] { new SalesReturnLineCompletion(Guid.NewGuid(), true, null) });

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
