using FusionOS.Modules.Procurement.Application.Suppliers.Commands.UpdateSupplier;
using FusionOS.Modules.Procurement.Application.Suppliers.Contracts;
using FusionOS.Modules.Procurement.Domain.Suppliers;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Procurement.Tests.Suppliers;

public class UpdateSupplierCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenSupplierExists_UpdatesAndReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var supplier = Supplier.Create(companyId, "Acme Supplies", "SUP-01");
        var repository = Substitute.For<ISupplierRepository>();
        repository.GetByIdAsync(companyId, supplier.Id, Arg.Any<CancellationToken>()).Returns(supplier);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new UpdateSupplierCommandHandler(repository, unitOfWork);
        var command = new UpdateSupplierCommand(companyId, supplier.Id, "Acme Supplies Ltd", "new@acme.com", "555-0100");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Name.Should().Be("Acme Supplies Ltd");
        result.ContactEmail.Should().Be("new@acme.com");
        result.Code.Should().Be("SUP-01");
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSupplierDoesNotExist_ThrowsKeyNotFoundException()
    {
        var companyId = Guid.NewGuid();
        var supplierId = Guid.NewGuid();
        var repository = Substitute.For<ISupplierRepository>();
        repository.GetByIdAsync(companyId, supplierId, Arg.Any<CancellationToken>()).Returns((Supplier?)null);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new UpdateSupplierCommandHandler(repository, unitOfWork);
        var command = new UpdateSupplierCommand(companyId, supplierId, "New Name", null, null);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
