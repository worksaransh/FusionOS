using FusionOS.Modules.Procurement.Application.SupplierContracts.Commands.CreateSupplierContract;
using FusionOS.Modules.Procurement.Application.SupplierContracts.Contracts;
using FusionOS.Modules.Procurement.Application.Suppliers.Contracts;
using FusionOS.Modules.Procurement.Domain.SupplierContracts;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Procurement.Tests.SupplierContracts;

public class CreateSupplierContractCommandHandlerTests
{
    private static readonly DateTimeOffset Start = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset End = new(2026, 12, 31, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Handle_WhenSupplierExists_PersistsContract()
    {
        var repository = Substitute.For<ISupplierContractRepository>();
        var supplierRepository = Substitute.For<ISupplierRepository>();
        supplierRepository.ExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateSupplierContractCommandHandler(repository, supplierRepository, unitOfWork);
        var command = new CreateSupplierContractCommand(Guid.NewGuid(), Guid.NewGuid(), Start, End, "Net 30 payment terms.");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Terms.Should().Be("Net 30 payment terms.");
        result.Status.Should().Be("Active");
        await repository.Received(1).AddAsync(Arg.Any<SupplierContract>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSupplierDoesNotExist_Throws()
    {
        var repository = Substitute.For<ISupplierContractRepository>();
        var supplierRepository = Substitute.For<ISupplierRepository>();
        supplierRepository.ExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateSupplierContractCommandHandler(repository, supplierRepository, unitOfWork);
        var command = new CreateSupplierContractCommand(Guid.NewGuid(), Guid.NewGuid(), Start, End, "Terms");

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<FusionOS.BuildingBlocks.Application.Exceptions.ValidationException>();
        await repository.DidNotReceive().AddAsync(Arg.Any<SupplierContract>(), Arg.Any<CancellationToken>());
    }
}
