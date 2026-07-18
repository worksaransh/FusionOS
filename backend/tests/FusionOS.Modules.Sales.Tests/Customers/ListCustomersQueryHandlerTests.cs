using FusionOS.Modules.Sales.Application.Customers.Contracts;
using FusionOS.Modules.Sales.Application.Customers.Queries.ListCustomers;
using FusionOS.Modules.Sales.Domain.Customers;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Sales.Tests.Customers;

public class ListCustomersQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsPagedCustomersForTheCompany()
    {
        var companyId = Guid.NewGuid();
        var customers = new[] { Customer.Create(companyId, "Acme Retail", "CUST-01") };
        var repository = Substitute.For<ICustomerRepository>();
        repository.ListAsync(companyId, null, 1, 25, Arg.Any<CancellationToken>()).Returns(customers);
        repository.CountAsync(companyId, null, Arg.Any<CancellationToken>()).Returns(1);
        var handler = new ListCustomersQueryHandler(repository);

        var result = await handler.Handle(new ListCustomersQuery(companyId), CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Data.Should().ContainSingle(c => c.Code == "CUST-01");
    }
}
