using FusionOS.Modules.Sales.Application.Customers.Commands.UpdateCustomer;
using FusionOS.Modules.Sales.Application.Customers.Contracts;
using FusionOS.Modules.Sales.Domain.Customers;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Sales.Tests.Customers;

public class UpdateCustomerCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenCustomerExists_UpdatesAndReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var customer = Customer.Create(companyId, "Acme Retail", "CUST-01");
        var repository = Substitute.For<ICustomerRepository>();
        repository.GetByIdAsync(companyId, customer.Id, Arg.Any<CancellationToken>()).Returns(customer);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new UpdateCustomerCommandHandler(repository, unitOfWork);
        var command = new UpdateCustomerCommand(companyId, customer.Id, "Acme Retail Inc", "new@acme.com", 5000m);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Name.Should().Be("Acme Retail Inc");
        result.CreditLimit.Should().Be(5000m);
        result.Code.Should().Be("CUST-01");
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCustomerDoesNotExist_ThrowsKeyNotFoundException()
    {
        var companyId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var repository = Substitute.For<ICustomerRepository>();
        repository.GetByIdAsync(companyId, customerId, Arg.Any<CancellationToken>()).Returns((Customer?)null);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new UpdateCustomerCommandHandler(repository, unitOfWork);
        var command = new UpdateCustomerCommand(companyId, customerId, "New Name", null, 0m);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
