using FusionOS.Modules.Core.Domain.Companies;
using FluentAssertions;
using Xunit;

namespace FusionOS.Modules.Core.Tests.Companies;

public class CompanyTests
{
    [Fact]
    public void Create_WithValidData_RaisesCompanyCreatedEvent()
    {
        var company = Company.Create("Acme Trading", "Acme Trading Pvt Ltd", "inr", "GSTIN123");

        company.Name.Should().Be("Acme Trading");
        company.BaseCurrency.Should().Be("INR");
        company.DomainEvents.Should().ContainSingle(e => e is Events.CompanyCreated);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithoutName_Throws(string invalidName)
    {
        var act = () => Company.Create(invalidName, "Legal Name", "USD");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithInvalidCurrencyLength_Throws()
    {
        var act = () => Company.Create("Acme", "Acme Ltd", "US");

        act.Should().Throw<ArgumentException>();
    }
}
