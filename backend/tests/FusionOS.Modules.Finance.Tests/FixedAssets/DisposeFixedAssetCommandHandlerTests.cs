using FluentAssertions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.FixedAssets.Commands.DisposeFixedAsset;
using FusionOS.Modules.Finance.Application.FixedAssets.Contracts;
using FusionOS.Modules.Finance.Application.JournalEntries.Contracts;
using FusionOS.Modules.Finance.Application.Settings.Contracts;
using FusionOS.Modules.Finance.Domain.FixedAssets;
using FusionOS.Modules.Finance.Domain.Settings;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.FixedAssets;

public class DisposeFixedAssetCommandHandlerTests
{
    private static DateTimeOffset AcquisitionDate => new(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static (
        DisposeFixedAssetCommandHandler handler,
        IJournalEntryRepository journalEntryRepository,
        IUnitOfWork unitOfWork,
        FixedAsset asset)
        BuildHandler(
            Guid companyId,
            Guid? accumulatedDepreciationAccountId,
            FinanceSettings? settings)
    {
        var asset = FixedAsset.Create(
            companyId, "FA-100", "Van", Guid.NewGuid(), accumulatedDepreciationAccountId, null,
            AcquisitionDate, 24000m, 4000m, 60);

        var repository = Substitute.For<IFixedAssetRepository>();
        repository.GetByIdAsync(companyId, asset.Id, Arg.Any<CancellationToken>()).Returns(asset);

        var settingsRepository = Substitute.For<IFinanceSettingsRepository>();
        settingsRepository.GetByCompanyIdAsync(companyId, Arg.Any<CancellationToken>()).Returns(settings);

        var journalEntryRepository = Substitute.For<IJournalEntryRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();

        var handler = new DisposeFixedAssetCommandHandler(repository, settingsRepository, journalEntryRepository, unitOfWork);
        return (handler, journalEntryRepository, unitOfWork, asset);
    }

    private static FinanceSettings ConfiguredSettings(Guid companyId, Guid? proceedsAccountId, Guid? gainLossAccountId)
    {
        var settings = FinanceSettings.CreateDefault(companyId);
        settings.ConfigureAccounts(null, null, null, null, proceedsAccountId, gainLossAccountId);
        return settings;
    }

    [Fact]
    public async Task Handle_WhenFixedAssetExists_DisposesAndReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var (handler, _, unitOfWork, asset) = BuildHandler(companyId, null, settings: null);
        var disposedDate = AcquisitionDate.AddYears(2);

        var result = await handler.Handle(new DisposeFixedAssetCommand(companyId, asset.Id, disposedDate, 0m), CancellationToken.None);

        result.FixedAsset.IsDisposed.Should().BeTrue();
        result.FixedAsset.DisposedDate.Should().Be(disposedDate);
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenFixedAssetDoesNotExist_ThrowsKeyNotFoundException()
    {
        var companyId = Guid.NewGuid();
        var fixedAssetId = Guid.NewGuid();
        var repository = Substitute.For<IFixedAssetRepository>();
        repository.GetByIdAsync(companyId, fixedAssetId, Arg.Any<CancellationToken>()).Returns((FixedAsset?)null);
        var handler = new DisposeFixedAssetCommandHandler(
            repository, Substitute.For<IFinanceSettingsRepository>(), Substitute.For<IJournalEntryRepository>(), Substitute.For<IUnitOfWork>());

        var act = () => handler.Handle(new DisposeFixedAssetCommand(companyId, fixedAssetId, AcquisitionDate.AddYears(1), 0m), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenDisposedDateBeforeAcquisitionDate_Throws()
    {
        var companyId = Guid.NewGuid();
        var (handler, _, _, asset) = BuildHandler(companyId, null, settings: null);

        var act = async () => await handler.Handle(new DisposeFixedAssetCommand(companyId, asset.Id, AcquisitionDate.AddDays(-1), 0m), CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Handle_WhenSettingsNotConfigured_DisposesButSkipsGlPosting()
    {
        var companyId = Guid.NewGuid();
        // Asset has an accumulated-depreciation account, but FinanceSettings has no
        // default disposal-proceeds/gain-loss accounts configured (settings is null,
        // same "not configured yet" shape GetFinanceSettingsQueryHandler defaults to).
        var (handler, journalEntryRepository, unitOfWork, asset) = BuildHandler(companyId, Guid.NewGuid(), settings: null);
        var disposedDate = AcquisitionDate.AddMonths(24);

        var result = await handler.Handle(new DisposeFixedAssetCommand(companyId, asset.Id, disposedDate, 20000m), CancellationToken.None);

        result.FixedAsset.IsDisposed.Should().BeTrue();
        result.JournalEntry.Should().BeNull();
        result.GainOrLoss.Should().NotBe(0m);
        await journalEntryRepository.DidNotReceive().AddAsync(Arg.Any<Domain.JournalEntries.JournalEntry>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenProceedsExceedNetBookValue_PostsBalancedEntryWithGainCredited()
    {
        var companyId = Guid.NewGuid();
        var accumulatedDepreciationAccountId = Guid.NewGuid();
        var proceedsAccountId = Guid.NewGuid();
        var gainLossAccountId = Guid.NewGuid();
        var settings = ConfiguredSettings(companyId, proceedsAccountId, gainLossAccountId);
        var (handler, journalEntryRepository, _, asset) = BuildHandler(companyId, accumulatedDepreciationAccountId, settings);
        var disposedDate = AcquisitionDate.AddMonths(24);
        var expectedAccumulatedDepreciation = Math.Round(asset.CalculateAccumulatedDepreciation(disposedDate), 4, MidpointRounding.AwayFromZero);
        var expectedNetBookValue = asset.AcquisitionCost - expectedAccumulatedDepreciation;
        var proceeds = expectedNetBookValue + 4000m; // proceeds above NBV => gain
        var expectedGain = proceeds - expectedNetBookValue;

        var result = await handler.Handle(new DisposeFixedAssetCommand(companyId, asset.Id, disposedDate, proceeds), CancellationToken.None);

        result.AccumulatedDepreciation.Should().Be(expectedAccumulatedDepreciation);
        result.NetBookValue.Should().Be(expectedNetBookValue);
        result.GainOrLoss.Should().Be(expectedGain);
        result.JournalEntry.Should().NotBeNull();
        result.JournalEntry!.Status.Should().Be("Posted");
        result.JournalEntry.TotalDebit.Should().Be(result.JournalEntry.TotalCredit);
        result.JournalEntry.Lines.Should().Contain(l => l.AccountId == gainLossAccountId && l.Credit == expectedGain);
        result.JournalEntry.Lines.Should().Contain(l => l.AccountId == asset.AssetAccountId && l.Credit == asset.AcquisitionCost);
        result.JournalEntry.Lines.Should().Contain(l => l.AccountId == accumulatedDepreciationAccountId && l.Debit == expectedAccumulatedDepreciation);
        result.JournalEntry.Lines.Should().Contain(l => l.AccountId == proceedsAccountId && l.Debit == proceeds);
        await journalEntryRepository.Received(1).AddAsync(Arg.Any<Domain.JournalEntries.JournalEntry>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenProceedsBelowNetBookValue_PostsBalancedEntryWithLossDebited()
    {
        var companyId = Guid.NewGuid();
        var accumulatedDepreciationAccountId = Guid.NewGuid();
        var proceedsAccountId = Guid.NewGuid();
        var gainLossAccountId = Guid.NewGuid();
        var settings = ConfiguredSettings(companyId, proceedsAccountId, gainLossAccountId);
        var (handler, journalEntryRepository, _, asset) = BuildHandler(companyId, accumulatedDepreciationAccountId, settings);
        var disposedDate = AcquisitionDate.AddMonths(24);
        var expectedAccumulatedDepreciation = Math.Round(asset.CalculateAccumulatedDepreciation(disposedDate), 4, MidpointRounding.AwayFromZero);
        var expectedNetBookValue = asset.AcquisitionCost - expectedAccumulatedDepreciation;
        var proceeds = Math.Max(expectedNetBookValue - 4000m, 0m); // proceeds below NBV => loss
        var expectedLoss = proceeds - expectedNetBookValue;

        var result = await handler.Handle(new DisposeFixedAssetCommand(companyId, asset.Id, disposedDate, proceeds), CancellationToken.None);

        result.GainOrLoss.Should().Be(expectedLoss);
        result.GainOrLoss.Should().BeLessThan(0m);
        result.JournalEntry.Should().NotBeNull();
        result.JournalEntry!.TotalDebit.Should().Be(result.JournalEntry.TotalCredit);
        result.JournalEntry.Lines.Should().Contain(l => l.AccountId == gainLossAccountId && l.Debit == -expectedLoss);
        await journalEntryRepository.Received(1).AddAsync(Arg.Any<Domain.JournalEntries.JournalEntry>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenZeroProceedsDiscard_PostsEntryWithoutProceedsLine()
    {
        var companyId = Guid.NewGuid();
        var accumulatedDepreciationAccountId = Guid.NewGuid();
        var gainLossAccountId = Guid.NewGuid();
        // No proceeds account configured — fine, since DisposalProceeds is 0 and no
        // proceeds line is ever needed for a pure discard.
        var settings = ConfiguredSettings(companyId, null, gainLossAccountId);
        var (handler, journalEntryRepository, _, asset) = BuildHandler(companyId, accumulatedDepreciationAccountId, settings);
        var disposedDate = AcquisitionDate.AddMonths(24);

        var result = await handler.Handle(new DisposeFixedAssetCommand(companyId, asset.Id, disposedDate, 0m), CancellationToken.None);

        result.JournalEntry.Should().NotBeNull();
        result.JournalEntry!.TotalDebit.Should().Be(result.JournalEntry.TotalCredit);
        result.JournalEntry.Lines.Should().NotContain(l => l.Debit > 0m && l.AccountId != accumulatedDepreciationAccountId && l.AccountId != gainLossAccountId);
        await journalEntryRepository.Received(1).AddAsync(Arg.Any<Domain.JournalEntries.JournalEntry>(), Arg.Any<CancellationToken>());
    }
}
