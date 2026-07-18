using FusionOS.Modules.Procurement.Application.Suppliers.Contracts;
using FusionOS.Modules.Procurement.Application.Suppliers.Queries.GetSupplierById;
using FusionOS.Modules.Procurement.Domain.Suppliers;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Procurement.Tests.Suppliers;

public class GetSupplierByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenSupplierExists_ReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var supplier = Supplier.Create(companyId, "Acme Supplies", "SUP-01");
        var repository = Substitute.For<ISupplierRepository>();
        repository.GetByIdAsync(companyId, supplier.Id, Arg.Any<CancellationToken>()).Returns(supplier);
        var handler = new GetSupplierByIdQueryHandler(repository);

        var result = await handler.Handle(new GetSupplierByIdQuery(companyId, supplier.Id), CancellationToken.None);

        result.Code.Should().Be("SUP-01");
    }

    [Fact]
    public async Task Handle_WhenSupplierDoesNotExist_ThrowsKeyNotFoundException()
    {
        var companyId = Guid.NewGuid();
        var supplierId = Guid.NewGuid();
        var repository = Substitute.For<ISupplierRepository>();
        repository.GetByIdAsync(companyId, supplierId, Arg.Any<CancellationToken>()).Returns((Supplier?)null);
        var handler = new GetSupplierByIdQueryHandler(repository);

        var act = () => handler.Handle(new GetSupplierByIdQuery(companyId, supplierId), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
