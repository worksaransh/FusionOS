using FusionOS.Modules.Sales.Application.Customers.Contracts;
using FusionOS.Modules.Sales.Application.Quotations.Commands.CreateQuotation;
using FusionOS.Modules.Sales.Application.Quotations.Contracts;
using FusionOS.Modules.Sales.Domain.Quotations;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Sales.Tests.Quotations;

public class CreateQuotationCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenCustomerExists_PersistsQuotation()
    {
        var repository = Substitute.For<IQuotationRepository>();
        var customerRepository = Substitute.For<ICustomerRepository>();
        customerRepository.ExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateQuotationCommandHandler(repository, customerRepository, unitOfWork);
        var command = new CreateQuotationCommand(Guid.NewGuid(), Guid.NewGuid(), new[] { new QuotationLineInput(Guid.NewGuid(), 2m, 50m) });

        var result = await handler.Handle(command, CancellationToken.None);

        result.TotalAmount.Should().Be(100m);
        await repository.Received(1).AddAsync(Arg.Any<Quotation>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCustomerDoesNotExist_Throws()
    {
        var repository = Substitute.For<IQuotationRepository>();
        var customerRepository = Substitute.For<ICustomerRepository>();
        customerRepository.ExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateQuotationCommandHandler(repository, customerRepository, unitOfWork);
        var command = new CreateQuotationCommand(Guid.NewGuid(), Guid.NewGuid(), new[] { new QuotationLineInput(Guid.NewGuid(), 2m, 50m) });

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<FusionOS.BuildingBlocks.Application.Exceptions.ValidationException>();
    }
}
