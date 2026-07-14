using FusionOS.Modules.Procurement.Application.Suppliers.Commands.CreateSupplier;
using FusionOS.Modules.Procurement.Application.Suppliers.Contracts;
using FusionOS.Modules.Procurement.Domain.Suppliers;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Procurement.Tests.Suppliers;

public class CreateSupplierCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenCodeIsUnique_PersistsSupplier()
    {
        var repository = Substitute.For<ISupplierRepository>();
        repository.CodeExistsAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateSupplierCommandHandler(repository, unitOfWork);
        var command = new CreateSupplierCommand(Guid.NewGuid(), "Acme Supplies", "SUP-01", null, null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Code.Should().Be("SUP-01");
        await repository.Received(1).AddAsync(Arg.Any<Supplier>(), Arg.Any<CancellationToken>());
    }
}
