using FusionOS.Modules.Procurement.Domain.Suppliers;
using FluentAssertions;
using Xunit;

namespace FusionOS.Modules.Procurement.Tests.Suppliers;

public class SupplierTests
{
    [Fact]
    public void Create_WithValidData_NormalizesCodeAndEmail()
    {
        var supplier = Supplier.Create(Guid.NewGuid(), "Acme Supplies", " sup-01 ", "Sales@Acme.com");

        supplier.Code.Should().Be("SUP-01");
        supplier.ContactEmail.Should().Be("sales@acme.com");
    }

    [Fact]
    public void Create_WithInvalidEmail_Throws()
    {
        var act = () => Supplier.Create(Guid.NewGuid(), "Acme Supplies", "SUP-01", "not-an-email");

        act.Should().Throw<ArgumentException>();
    }
}
