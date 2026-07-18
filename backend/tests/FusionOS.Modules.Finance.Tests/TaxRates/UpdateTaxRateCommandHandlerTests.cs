using FluentAssertions;
using FusionOS.Modules.Finance.Application.TaxRates.Commands.UpdateTaxRate;
using FusionOS.Modules.Finance.Application.TaxRates.Contracts;
using FusionOS.Modules.Finance.Domain.TaxRates;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.TaxRates;

public class UpdateTaxRateCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenTaxRateExists_UpdatesAndReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var taxRate = TaxRate.Create(companyId, Guid.NewGuid(), "GST-STANDARD", "GST 18%", 18.00m);
        var repository = Substitute.For<ITaxRateRepository>();
        repository.GetByIdAsync(companyId, taxRate.Id, Arg.Any<CancellationToken>()).Returns(taxRate);
        var unitOfWork = Substitute.For<FusionOS.Modules.Finance.Application.Accounts.Contracts.IUnitOfWork>();
        var handler = new UpdateTaxRateCommandHandler(repository, unitOfWork);
        var command = new UpdateTaxRateCommand(companyId, taxRate.Id, "GST 18% (Standard)", 18.50m);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Name.Should().Be("GST 18% (Standard)");
        result.Percentage.Should().Be(18.50m);
        result.Code.Should().Be("GST-STANDARD");
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
        var handler = new UpdateTaxRateCommandHandler(repository, unitOfWork);
        var command = new UpdateTaxRateCommand(companyId, taxRateId, "GST 18%", 18.00m);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
