using FusionOS.Modules.Sales.Application.Customers.Contracts;
using FusionOS.Modules.Sales.Application.Customers.Queries.GetCustomerById;
using FusionOS.Modules.Sales.Domain.Customers;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Sales.Tests.Customers;

public class GetCustomerByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenCustomerExists_ReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var customer = Customer.Create(companyId, "Acme Retail", "CUST-01");
        var repository = Substitute.For<ICustomerRepository>();
        repository.GetByIdAsync(companyId, customer.Id, Arg.Any<CancellationToken>()).Returns(customer);
        var handler = new GetCustomerByIdQueryHandler(repository);

        var result = await handler.Handle(new GetCustomerByIdQuery(companyId, customer.Id), CancellationToken.None);

        result.Code.Should().Be("CUST-01");
    }

    [Fact]
    public async Task Handle_WhenCustomerDoesNotExist_ThrowsKeyNotFoundException()
    {
        var companyId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var repository = Substitute.For<ICustomerRepository>();
        repository.GetByIdAsync(companyId, customerId, Arg.Any<CancellationToken>()).Returns((Customer?)null);
        var handler = new GetCustomerByIdQueryHandler(repository);

        var act = () => handler.Handle(new GetCustomerByIdQuery(companyId, customerId), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
