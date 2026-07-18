using FusionOS.Modules.Procurement.Application.Suppliers.Contracts;
using FusionOS.Modules.Procurement.Application.Suppliers.Queries.ListSuppliers;
using FusionOS.Modules.Procurement.Domain.Suppliers;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Procurement.Tests.Suppliers;

public class ListSuppliersQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsPagedSuppliersForTheCompany()
    {
        var companyId = Guid.NewGuid();
        var suppliers = new[] { Supplier.Create(companyId, "Acme Supplies", "SUP-01") };
        var repository = Substitute.For<ISupplierRepository>();
        repository.ListAsync(companyId, null, 1, 25, Arg.Any<CancellationToken>()).Returns(suppliers);
        repository.CountAsync(companyId, null, Arg.Any<CancellationToken>()).Returns(1);
        var handler = new ListSuppliersQueryHandler(repository);

        var result = await handler.Handle(new ListSuppliersQuery(companyId), CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Data.Should().ContainSingle(s => s.Code == "SUP-01");
    }
}
