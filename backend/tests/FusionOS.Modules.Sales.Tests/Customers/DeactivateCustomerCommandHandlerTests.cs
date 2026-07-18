using FusionOS.Modules.Sales.Application.Customers.Commands.DeactivateCustomer;
using FusionOS.Modules.Sales.Application.Customers.Contracts;
using FusionOS.Modules.Sales.Domain.Customers;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Sales.Tests.Customers;

public class DeactivateCustomerCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenCustomerExists_DeactivatesAndReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var customer = Customer.Create(companyId, "Acme Retail", "CUST-01");
        var repository = Substitute.For<ICustomerRepository>();
        repository.GetByIdAsync(companyId, customer.Id, Arg.Any<CancellationToken>()).Returns(customer);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new DeactivateCustomerCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(new DeactivateCustomerCommand(companyId, customer.Id), CancellationToken.None);

        result.IsActive.Should().BeFalse();
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
        var handler = new DeactivateCustomerCommandHandler(repository, unitOfWork);

        var act = () => handler.Handle(new DeactivateCustomerCommand(companyId, customerId), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
