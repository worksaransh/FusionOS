using FluentAssertions;
using FusionOS.Modules.Finance.Application.TaxJurisdictions.Commands.DeactivateTaxJurisdiction;
using FusionOS.Modules.Finance.Application.TaxJurisdictions.Contracts;
using FusionOS.Modules.Finance.Domain.TaxJurisdictions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.TaxJurisdictions;

public class DeactivateTaxJurisdictionCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenTaxJurisdictionExists_DeactivatesAndReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var jurisdiction = TaxJurisdiction.Create(companyId, "IN-KA", "Karnataka, India");
        var repository = Substitute.For<ITaxJurisdictionRepository>();
        repository.GetByIdAsync(companyId, jurisdiction.Id, Arg.Any<CancellationToken>()).Returns(jurisdiction);
        var unitOfWork = Substitute.For<FusionOS.Modules.Finance.Application.Accounts.Contracts.IUnitOfWork>();
        var handler = new DeactivateTaxJurisdictionCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(new DeactivateTaxJurisdictionCommand(companyId, jurisdiction.Id), CancellationToken.None);

        result.IsActive.Should().BeFalse();
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTaxJurisdictionDoesNotExist_ThrowsKeyNotFoundException()
    {
        var companyId = Guid.NewGuid();
        var jurisdictionId = Guid.NewGuid();
        var repository = Substitute.For<ITaxJurisdictionRepository>();
        repository.GetByIdAsync(companyId, jurisdictionId, Arg.Any<CancellationToken>()).Returns((TaxJurisdiction?)null);
        var unitOfWork = Substitute.For<FusionOS.Modules.Finance.Application.Accounts.Contracts.IUnitOfWork>();
        var handler = new DeactivateTaxJurisdictionCommandHandler(repository, unitOfWork);

        var act = () => handler.Handle(new DeactivateTaxJurisdictionCommand(companyId, jurisdictionId), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
