using FluentAssertions;
using FusionOS.Modules.Finance.Application.TaxRates.Commands.DeactivateTaxRate;
using FusionOS.Modules.Finance.Application.TaxRates.Contracts;
using FusionOS.Modules.Finance.Domain.TaxRates;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.TaxRates;

public class DeactivateTaxRateCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenTaxRateExists_DeactivatesAndReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var taxRate = TaxRate.Create(companyId, Guid.NewGuid(), "GST-STANDARD", "GST 18%", 18.00m);
        var repository = Substitute.For<ITaxRateRepository>();
        repository.GetByIdAsync(companyId, taxRate.Id, Arg.Any<CancellationToken>()).Returns(taxRate);
        var unitOfWork = Substitute.For<FusionOS.Modules.Finance.Application.Accounts.Contracts.IUnitOfWork>();
        var handler = new DeactivateTaxRateCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(new DeactivateTaxRateCommand(companyId, taxRate.Id), CancellationToken.None);

        result.IsActive.Should().BeFalse();
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTaxRateDoesNotExist_ThrowsKeyNotFoundException()
    {
        var companyId = Guid.NewGuid();
        var taxRateId = Guid.NewGuid();
        var repository = Substitute.For<ITaxRateRepository>();
        repository.GetByIdAsync(companyId, taxRateId, Arg.Any<CancellationToken>()).Returns((TaxRate?)null);
        var unitOfWork = Substitute.For<FusionOS.Modules.Finance.Application.Accounts.Contracts.IUnitOfWork>();
        var handler = new DeactivateTaxRateCommandHandler(repository, unitOfWork);

        var act = () => handler.Handle(new DeactivateTaxRateCommand(companyId, taxRateId), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
