using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.Settings.Contracts;
using FusionOS.Modules.Finance.Application.Settings.Queries.GetFinanceSettings;
using FusionOS.Modules.Finance.Domain.Settings;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.Settings;

public class GetFinanceSettingsQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenSettingsAlreadyExist_ReturnsThemWithoutCreating()
    {
        var companyId = Guid.NewGuid();
        var existing = FinanceSettings.CreateDefault(companyId);
        var arAccountId = Guid.NewGuid();
        existing.ConfigureAccounts(arAccountId, null, null, null);
        var repository = Substitute.For<IFinanceSettingsRepository>();
        repository.GetByCompanyIdAsync(companyId, Arg.Any<CancellationToken>()).Returns(existing);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new GetFinanceSettingsQueryHandler(repository, unitOfWork);

        var result = await handler.Handle(new GetFinanceSettingsQuery(companyId), CancellationToken.None);

        result.DefaultArAccountId.Should().Be(arAccountId);
        await repository.DidNotReceive().AddAsync(Arg.Any<FinanceSettings>(), Arg.Any<CancellationToken>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSettingsDoNotExistYet_CreatesDefaultsAndPersists()
    {
        var companyId = Guid.NewGuid();
        var repository = Substitute.For<IFinanceSettingsRepository>();
        repository.GetByCompanyIdAsync(companyId, Arg.Any<CancellationToken>()).Returns((FinanceSettings?)null);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new GetFinanceSettingsQueryHandler(repository, unitOfWork);

        var result = await handler.Handle(new GetFinanceSettingsQuery(companyId), CancellationToken.None);

        result.CompanyId.Should().Be(companyId);
        result.DefaultArAccountId.Should().BeNull();
        await repository.Received(1).AddAsync(Arg.Any<FinanceSettings>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
