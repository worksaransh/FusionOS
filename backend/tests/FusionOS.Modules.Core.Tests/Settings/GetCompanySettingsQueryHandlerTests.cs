using FusionOS.Modules.Core.Application.Companies.Contracts;
using FusionOS.Modules.Core.Application.Settings.Contracts;
using FusionOS.Modules.Core.Application.Settings.Queries.GetCompanySettings;
using FusionOS.Modules.Core.Domain.Settings;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Core.Tests.Settings;

/// <summary>
/// Covers the get-or-create behavior (Phase M5, 2026-07-15): every company has
/// settings from the moment they're first read, even if nobody has ever
/// touched the Settings page — same established pattern as
/// IUserRepository.GetOrCreateCompanyOwnerRoleAsync, just at the handler layer.
/// </summary>
public class GetCompanySettingsQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenSettingsAlreadyExist_ReturnsThemWithoutCreating()
    {
        var companyId = Guid.NewGuid();
        var existing = CompanySettings.CreateDefault(companyId);
        existing.UpdateSettings("INR", 50, "Acme", null);
        var repository = Substitute.For<ICompanySettingsRepository>();
        repository.GetByCompanyIdAsync(companyId, Arg.Any<CancellationToken>()).Returns(existing);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new GetCompanySettingsQueryHandler(repository, unitOfWork);

        var result = await handler.Handle(new GetCompanySettingsQuery(companyId), CancellationToken.None);

        result.DefaultCurrency.Should().Be("INR");
        result.DefaultPageSize.Should().Be(50);
        result.DisplayName.Should().Be("Acme");
        await repository.DidNotReceive().AddAsync(Arg.Any<CompanySettings>(), Arg.Any<CancellationToken>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSettingsDoNotExist_CreatesDefaultsAndPersistsThem()
    {
        var companyId = Guid.NewGuid();
        var repository = Substitute.For<ICompanySettingsRepository>();
        repository.GetByCompanyIdAsync(companyId, Arg.Any<CancellationToken>()).Returns((CompanySettings?)null);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new GetCompanySettingsQueryHandler(repository, unitOfWork);

        var result = await handler.Handle(new GetCompanySettingsQuery(companyId), CancellationToken.None);

        result.CompanyId.Should().Be(companyId);
        result.DefaultCurrency.Should().Be("USD");
        result.DefaultPageSize.Should().Be(25);
        await repository.Received(1).AddAsync(Arg.Any<CompanySettings>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
