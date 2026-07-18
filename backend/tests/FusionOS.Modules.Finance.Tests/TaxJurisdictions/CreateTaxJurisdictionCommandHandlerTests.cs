using FluentAssertions;
using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Finance.Application.TaxJurisdictions.Commands.CreateTaxJurisdiction;
using FusionOS.Modules.Finance.Application.TaxJurisdictions.Contracts;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.TaxJurisdictions;

public class CreateTaxJurisdictionCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenCodeIsNew_PersistsTaxJurisdiction()
    {
        var repository = Substitute.For<ITaxJurisdictionRepository>();
        repository.CodeExistsAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        var unitOfWork = Substitute.For<FusionOS.Modules.Finance.Application.Accounts.Contracts.IUnitOfWork>();
        var handler = new CreateTaxJurisdictionCommandHandler(repository, unitOfWork);
        var command = new CreateTaxJurisdictionCommand(Guid.NewGuid(), "IN-KA", "Karnataka, India");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Code.Should().Be("IN-KA");
        await repository.Received(1).AddAsync(Arg.Any<FusionOS.Modules.Finance.Domain.TaxJurisdictions.TaxJurisdiction>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCodeAlreadyExists_Throws()
    {
        var repository = Substitute.For<ITaxJurisdictionRepository>();
        repository.CodeExistsAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(true);
        var unitOfWork = Substitute.For<FusionOS.Modules.Finance.Application.Accounts.Contracts.IUnitOfWork>();
        var handler = new CreateTaxJurisdictionCommandHandler(repository, unitOfWork);
        var command = new CreateTaxJurisdictionCommand(Guid.NewGuid(), "IN-KA", "Karnataka, India");

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
