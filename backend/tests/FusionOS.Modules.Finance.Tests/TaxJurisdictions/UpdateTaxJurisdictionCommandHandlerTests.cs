using FluentAssertions;
using FusionOS.Modules.Finance.Application.TaxJurisdictions.Commands.UpdateTaxJurisdiction;
using FusionOS.Modules.Finance.Application.TaxJurisdictions.Contracts;
using FusionOS.Modules.Finance.Domain.TaxJurisdictions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.TaxJurisdictions;

public class UpdateTaxJurisdictionCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenTaxJurisdictionExists_UpdatesAndReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var jurisdiction = TaxJurisdiction.Create(companyId, "IN-KA", "Karnataka, India");
        var repository = Substitute.For<ITaxJurisdictionRepository>();
        repository.GetByIdAsync(companyId, jurisdiction.Id, Arg.Any<CancellationToken>()).Returns(jurisdiction);
        var unitOfWork = Substitute.For<FusionOS.Modules.Finance.Application.Accounts.Contracts.IUnitOfWork>();
        var handler = new UpdateTaxJurisdictionCommandHandler(repository, unitOfWork);
        var command = new UpdateTaxJurisdictionCommand(companyId, jurisdiction.Id, "Karnataka State, India");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Name.Should().Be("Karnataka State, India");
        result.Code.Should().Be("IN-KA");
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
        var handler = new UpdateTaxJurisdictionCommandHandler(repository, unitOfWork);
        var command = new UpdateTaxJurisdictionCommand(companyId, jurisdictionId, "Karnataka, India");

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
