using FusionOS.Modules.Sales.Application.Customers.Commands.CreateCustomer;
using FusionOS.Modules.Sales.Application.Customers.Contracts;
using FusionOS.Modules.Sales.Domain.Customers;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Sales.Tests.Customers;

public class CreateCustomerCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenCodeIsUnique_PersistsCustomer()
    {
        var repository = Substitute.For<ICustomerRepository>();
        repository.CodeExistsAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateCustomerCommandHandler(repository, unitOfWork);
        var command = new CreateCustomerCommand(Guid.NewGuid(), "Acme Retail", "CUST-01", null, 0m);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Code.Should().Be("CUST-01");
        await repository.Received(1).AddAsync(Arg.Any<Customer>(), Arg.Any<CancellationToken>());
    }
}
