using System.Text.Json;
using FluentAssertions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.IntegrationEvents.Consumers;
using FusionOS.Modules.Finance.Application.JournalEntries.Contracts;
using FusionOS.Modules.Finance.Application.Receivables.Contracts;
using FusionOS.Modules.Finance.Application.Settings.Contracts;
using FusionOS.Modules.Finance.Domain.JournalEntries;
using FusionOS.Modules.Finance.Domain.Receivables;
using FusionOS.SharedKernel.Events;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.IntegrationEvents.Consumers;

/// <summary>Covers InvoiceIssuedConsumer's GL-posting addition (Phase 2 closeout, 2026-07-18) — the pre-existing AR-charge behavior plus the new conditional JournalEntry post.</summary>
public class InvoiceIssuedConsumerTests
{
    private static string BuildPayload(Guid invoiceId, Guid companyId, Guid customerId, decimal totalAmount) =>
        JsonSerializer.Serialize(new { InvoiceId = invoiceId, CompanyId = companyId, SalesOrderId = Guid.NewGuid(), CustomerId = customerId, TotalAmount = totalAmount });

    [Fact]
    public async Task HandleAsync_WhenFinanceSettingsConfigured_PostsBalancedJournalEntry()
    {
        var ledgerRepository = Substitute.For<IArLedgerRepository>();
        var journalEntryRepository = Substitute.For<IJournalEntryRepository>();
        var settingsRepository = Substitute.For<IFinanceSettingsRepository>();
        var processedEvents = Substitute.For<IProcessedIntegrationEventStore>();
        processedEvents.HasProcessedAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        var unitOfWork = Substitute.For<IUnitOfWork>();

        var companyId = Guid.NewGuid();
        var arAccountId = Guid.NewGuid();
        var revenueAccountId = Guid.NewGuid();
        settingsRepository.GetByCompanyIdAsync(companyId, Arg.Any<CancellationToken>())
            .Returns(BuildSettings(companyId, arAccountId, revenueAccountId, null, null));

        var consumer = new InvoiceIssuedConsumer(ledgerRepository, journalEntryRepository, settingsRepository, processedEvents, unitOfWork);
        var payload = BuildPayload(Guid.NewGuid(), companyId, Guid.NewGuid(), 500m);

        await consumer.HandleAsync(Guid.NewGuid(), companyId, payload, CancellationToken.None);

        await ledgerRepository.Received(1).AddAsync(Arg.Any<ArLedgerEntry>(), Arg.Any<CancellationToken>());
        await journalEntryRepository.Received(1).AddAsync(
            Arg.Is<JournalEntry>(e => e.Status == JournalEntryStatus.Posted && e.TotalDebit == 500m && e.TotalCredit == 500m
                && e.Lines.Any(l => l.AccountId == arAccountId && l.Debit == 500m)
                && e.Lines.Any(l => l.AccountId == revenueAccountId && l.Credit == 500m)),
            Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenFinanceSettingsNotConfigured_SkipsJournalEntryButStillRecordsCharge()
    {
        var ledgerRepository = Substitute.For<IArLedgerRepository>();
        var journalEntryRepository = Substitute.For<IJournalEntryRepository>();
        var settingsRepository = Substitute.For<IFinanceSettingsRepository>();
        settingsRepository.GetByCompanyIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((FusionOS.Modules.Finance.Domain.Settings.FinanceSettings?)null);
        var processedEvents = Substitute.For<IProcessedIntegrationEventStore>();
        processedEvents.HasProcessedAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        var unitOfWork = Substitute.For<IUnitOfWork>();

        var consumer = new InvoiceIssuedConsumer(ledgerRepository, journalEntryRepository, settingsRepository, processedEvents, unitOfWork);
        var payload = BuildPayload(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 500m);

        await consumer.HandleAsync(Guid.NewGuid(), Guid.NewGuid(), payload, CancellationToken.None);

        await ledgerRepository.Received(1).AddAsync(Arg.Any<ArLedgerEntry>(), Arg.Any<CancellationToken>());
        await journalEntryRepository.DidNotReceive().AddAsync(Arg.Any<JournalEntry>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenAlreadyProcessed_DoesNothing()
    {
        var ledgerRepository = Substitute.For<IArLedgerRepository>();
        var journalEntryRepository = Substitute.For<IJournalEntryRepository>();
        var settingsRepository = Substitute.For<IFinanceSettingsRepository>();
        var processedEvents = Substitute.For<IProcessedIntegrationEventStore>();
        processedEvents.HasProcessedAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);
        var unitOfWork = Substitute.For<IUnitOfWork>();

        var consumer = new InvoiceIssuedConsumer(ledgerRepository, journalEntryRepository, settingsRepository, processedEvents, unitOfWork);
        var payload = BuildPayload(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 500m);

        await consumer.HandleAsync(Guid.NewGuid(), Guid.NewGuid(), payload, CancellationToken.None);

        await ledgerRepository.DidNotReceive().AddAsync(Arg.Any<ArLedgerEntry>(), Arg.Any<CancellationToken>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private static FusionOS.Modules.Finance.Domain.Settings.FinanceSettings BuildSettings(Guid companyId, Guid? arAccountId, Guid? revenueAccountId, Guid? apAccountId, Guid? expenseAccountId)
    {
        var settings = FusionOS.Modules.Finance.Domain.Settings.FinanceSettings.CreateDefault(companyId);
        settings.ConfigureAccounts(arAccountId, revenueAccountId, apAccountId, expenseAccountId);
        return settings;
    }
}
