using FusionOS.Modules.Procurement.Application.SupplierContracts.Commands.TerminateSupplierContract;
using FusionOS.Modules.Procurement.Application.SupplierContracts.Contracts;
using FusionOS.Modules.Procurement.Domain.SupplierContracts;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Procurement.Tests.SupplierContracts;

public class TerminateSupplierContractCommandHandlerTests
{
    private static readonly DateTimeOffset Start = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset End = new(2026, 12, 31, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Handle_WithExistingActiveContract_Terminates()
    {
        var companyId = Guid.NewGuid();
        var contract = SupplierContract.Create(companyId, Guid.NewGuid(), Start, End, "Terms");
        var repository = Substitute.For<ISupplierContractRepository>();
        repository.GetByIdAsync(companyId, contract.Id, Arg.Any<CancellationToken>()).Returns(contract);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new TerminateSupplierContractCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(new TerminateSupplierContractCommand(companyId, contract.Id), CancellationToken.None);

        result.Status.Should().Be("Terminated");
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenContractNotFound_Throws()
    {
        var companyId = Guid.NewGuid();
        var repository = Substitute.For<ISupplierContractRepository>();
        repository.GetByIdAsync(companyId, Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((SupplierContract?)null);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new TerminateSupplierContractCommandHandler(repository, unitOfWork);

        var act = async () => await handler.Handle(new TerminateSupplierContractCommand(companyId, Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
