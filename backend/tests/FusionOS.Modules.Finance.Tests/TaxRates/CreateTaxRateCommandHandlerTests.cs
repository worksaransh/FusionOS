using FluentAssertions;
using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Finance.Application.TaxRates.Commands.CreateTaxRate;
using FusionOS.Modules.Finance.Application.TaxRates.Contracts;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.TaxRates;

public class CreateTaxRateCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenJurisdictionExistsAndCodeIsNew_PersistsTaxRate()
    {
        var repository = Substitute.For<ITaxRateRepository>();
        repository.TaxJurisdictionExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);
        repository.CodeExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);
        var unitOfWork = Substitute.For<FusionOS.Modules.Finance.Application.Accounts.Contracts.IUnitOfWork>();
        var handler = new CreateTaxRateCommandHandler(repository, unitOfWork);
        var command = new CreateTaxRateCommand(Guid.NewGuid(), Guid.NewGuid(), "GST-STANDARD", "GST 18%", 18.00m);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Code.Should().Be("GST-STANDARD");
        await repository.Received(1).AddAsync(Arg.Any<FusionOS.Modules.Finance.Domain.TaxRates.TaxRate>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTaxJurisdictionDoesNotExist_Throws()
    {
        var repository = Substitute.For<ITaxRateRepository>();
        repository.TaxJurisdictionExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        var unitOfWork = Substitute.For<FusionOS.Modules.Finance.Application.Accounts.Contracts.IUnitOfWork>();
        var handler = new CreateTaxRateCommandHandler(repository, unitOfWork);
        var command = new CreateTaxRateCommand(Guid.NewGuid(), Guid.NewGuid(), "GST-STANDARD", "GST 18%", 18.00m);

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WhenCodeAlreadyExistsInJurisdiction_Throws()
    {
        var repository = Substitute.For<ITaxRateRepository>();
        repository.TaxJurisdictionExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);
        repository.CodeExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(true);
        var unitOfWork = Substitute.For<FusionOS.Modules.Finance.Application.Accounts.Contracts.IUnitOfWork>();
        var handler = new CreateTaxRateCommandHandler(repository, unitOfWork);
        var command = new CreateTaxRateCommand(Guid.NewGuid(), Guid.NewGuid(), "GST-STANDARD", "GST 18%", 18.00m);

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
