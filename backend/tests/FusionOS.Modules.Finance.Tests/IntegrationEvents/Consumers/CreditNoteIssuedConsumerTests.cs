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

/// <summary>Covers CreditNoteIssuedConsumer's GL-posting addition (Phase 2 closeout, 2026-07-18) — the reversed Debit Revenue / Credit AR entry, mirroring InvoiceIssuedConsumerTests.</summary>
public class CreditNoteIssuedConsumerTests
{
    private static string BuildPayload(Guid creditNoteId, Guid companyId, Guid invoiceId, Guid customerId, decimal totalAmount) =>
        JsonSerializer.Serialize(new { CreditNoteId = creditNoteId, CompanyId = companyId, InvoiceId = invoiceId, CustomerId = customerId, TotalAmount = totalAmount });

    [Fact]
    public async Task HandleAsync_WhenFinanceSettingsConfigured_PostsReversedJournalEntry()
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
        var settings = FusionOS.Modules.Finance.Domain.Settings.FinanceSettings.CreateDefault(companyId);
        settings.ConfigureAccounts(arAccountId, revenueAccountId, null, null);
        settingsRepository.GetByCompanyIdAsync(companyId, Arg.Any<CancellationToken>()).Returns(settings);

        var consumer = new CreditNoteIssuedConsumer(ledgerRepository, journalEntryRepository, settingsRepository, processedEvents, unitOfWork);
        var payload = BuildPayload(Guid.NewGuid(), companyId, Guid.NewGuid(), Guid.NewGuid(), 200m);

        await consumer.HandleAsync(Guid.NewGuid(), companyId, payload, CancellationToken.None);

        await ledgerRepository.Received(1).AddAsync(Arg.Any<ArLedgerEntry>(), Arg.Any<CancellationToken>());
        await journalEntryRepository.Received(1).AddAsync(
            Arg.Is<JournalEntry>(e => e.Status == JournalEntryStatus.Posted
                && e.Lines.Any(l => l.AccountId == revenueAccountId && l.Debit == 200m)
                && e.Lines.Any(l => l.AccountId == arAccountId && l.Credit == 200m)),
            Arg.Any<CancellationToken>());
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

        var consumer = new CreditNoteIssuedConsumer(ledgerRepository, journalEntryRepository, settingsRepository, processedEvents, unitOfWork);
        var payload = BuildPayload(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 200m);

        await consumer.HandleAsync(Guid.NewGuid(), Guid.NewGuid(), payload, CancellationToken.None);

        await ledgerRepository.Received(1).AddAsync(Arg.Any<ArLedgerEntry>(), Arg.Any<CancellationToken>());
        await journalEntryRepository.DidNotReceive().AddAsync(Arg.Any<JournalEntry>(), Arg.Any<CancellationToken>());
    }
}
