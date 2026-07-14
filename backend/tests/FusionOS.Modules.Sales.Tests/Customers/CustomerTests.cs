using FusionOS.Modules.Sales.Domain.Customers;
using FluentAssertions;
using Xunit;

namespace FusionOS.Modules.Sales.Tests.Customers;

public class CustomerTests
{
    [Fact]
    public void Create_WithValidData_NormalizesCode()
    {
        var customer = Customer.Create(Guid.NewGuid(), "Acme Retail", " cust-01 ", "buyer@acme.com", 50000m);

        customer.Code.Should().Be("CUST-01");
        customer.CreditLimit.Should().Be(50000m);
    }

    [Fact]
    public void Create_WithNegativeCreditLimit_Throws()
    {
        var act = () => Customer.Create(Guid.NewGuid(), "Acme Retail", "CUST-01", null, -1m);

        act.Should().Throw<ArgumentException>();
    }
}
