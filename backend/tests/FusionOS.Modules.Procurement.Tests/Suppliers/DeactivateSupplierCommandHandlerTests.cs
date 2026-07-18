using FusionOS.Modules.Procurement.Application.Suppliers.Commands.DeactivateSupplier;
using FusionOS.Modules.Procurement.Application.Suppliers.Contracts;
using FusionOS.Modules.Procurement.Domain.Suppliers;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Procurement.Tests.Suppliers;

public class DeactivateSupplierCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenSupplierExists_DeactivatesAndReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var supplier = Supplier.Create(companyId, "Acme Supplies", "SUP-01");
        var repository = Substitute.For<ISupplierRepository>();
        repository.GetByIdAsync(companyId, supplier.Id, Arg.Any<CancellationToken>()).Returns(supplier);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new DeactivateSupplierCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(new DeactivateSupplierCommand(companyId, supplier.Id), CancellationToken.None);

        result.IsActive.Should().BeFalse();
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
        var handler = new DeactivateSupplierCommandHandler(repository, unitOfWork);

        var act = () => handler.Handle(new DeactivateSupplierCommand(companyId, supplierId), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
