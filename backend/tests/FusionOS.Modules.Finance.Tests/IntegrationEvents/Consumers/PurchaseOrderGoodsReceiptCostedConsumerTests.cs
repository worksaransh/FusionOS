using System.Text.Json;
using FluentAssertions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.IntegrationEvents.Consumers;
using FusionOS.Modules.Finance.Application.JournalEntries.Contracts;
using FusionOS.Modules.Finance.Application.Payables.Contracts;
using FusionOS.Modules.Finance.Application.Settings.Contracts;
using FusionOS.Modules.Finance.Domain.JournalEntries;
using FusionOS.Modules.Finance.Domain.Payables;
using FusionOS.SharedKernel.Events;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.IntegrationEvents.Consumers;

/// <summary>Covers PurchaseOrderGoodsReceiptCostedConsumer's GL-posting addition (Phase 2 closeout, 2026-07-18) — Debit Purchase Expense / Credit AP, mirroring InvoiceIssuedConsumerTests.</summary>
public class PurchaseOrderGoodsReceiptCostedConsumerTests
{
    private static string BuildPayload(Guid purchaseOrderId, Guid companyId, Guid supplierId, decimal lineAmount) =>
        JsonSerializer.Serialize(new
        {
            PurchaseOrderId = purchaseOrderId,
            CompanyId = companyId,
            SupplierId = supplierId,
            ProductId = Guid.NewGuid(),
            QuantityReceived = 10m,
            UnitCost = lineAmount / 10m,
            LineAmount = lineAmount,
        });

    [Fact]
    public async Task HandleAsync_WhenFinanceSettingsConfigured_PostsBalancedJournalEntry()
    {
        var ledgerRepository = Substitute.For<IApLedgerRepository>();
        var factRepository = Substitute.For<IPurchaseOrderFactRepository>();
        var journalEntryRepository = Substitute.For<IJournalEntryRepository>();
        var settingsRepository = Substitute.For<IFinanceSettingsRepository>();
        var processedEvents = Substitute.For<IProcessedIntegrationEventStore>();
        processedEvents.HasProcessedAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        var unitOfWork = Substitute.For<IUnitOfWork>();

        var companyId = Guid.NewGuid();
        var expenseAccountId = Guid.NewGuid();
        var apAccountId = Guid.NewGuid();
        var settings = FusionOS.Modules.Finance.Domain.Settings.FinanceSettings.CreateDefault(companyId);
        settings.ConfigureAccounts(null, null, apAccountId, expenseAccountId);
        settingsRepository.GetByCompanyIdAsync(companyId, Arg.Any<CancellationToken>()).Returns(settings);

        var consumer = new PurchaseOrderGoodsReceiptCostedConsumer(ledgerRepository, factRepository, journalEntryRepository, settingsRepository, processedEvents, unitOfWork);
        var payload = BuildPayload(Guid.NewGuid(), companyId, Guid.NewGuid(), 800m);

        await consumer.HandleAsync(Guid.NewGuid(), companyId, payload, CancellationToken.None);

        await ledgerRepository.Received(1).AddAsync(Arg.Any<ApLedgerEntry>(), Arg.Any<CancellationToken>());
        await journalEntryRepository.Received(1).AddAsync(
            Arg.Is<JournalEntry>(e => e.Status == JournalEntryStatus.Posted
                && e.Lines.Any(l => l.AccountId == expenseAccountId && l.Debit == 800m)
                && e.Lines.Any(l => l.AccountId == apAccountId && l.Credit == 800m)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenFinanceSettingsNotConfigured_SkipsJournalEntryButStillRecordsCharge()
    {
        var ledgerRepository = Substitute.For<IApLedgerRepository>();
        var factRepository = Substitute.For<IPurchaseOrderFactRepository>();
        var journalEntryRepository = Substitute.For<IJournalEntryRepository>();
        var settingsRepository = Substitute.For<IFinanceSettingsRepository>();
        settingsRepository.GetByCompanyIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((FusionOS.Modules.Finance.Domain.Settings.FinanceSettings?)null);
        var processedEvents = Substitute.For<IProcessedIntegrationEventStore>();
        processedEvents.HasProcessedAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        var unitOfWork = Substitute.For<IUnitOfWork>();

        var consumer = new PurchaseOrderGoodsReceiptCostedConsumer(ledgerRepository, factRepository, journalEntryRepository, settingsRepository, processedEvents, unitOfWork);
        var payload = BuildPayload(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 800m);

        await consumer.HandleAsync(Guid.NewGuid(), Guid.NewGuid(), payload, CancellationToken.None);

        await ledgerRepository.Received(1).AddAsync(Arg.Any<ApLedgerEntry>(), Arg.Any<CancellationToken>());
        await journalEntryRepository.DidNotReceive().AddAsync(Arg.Any<JournalEntry>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_AccumulatesReceivedAmountIntoThePurchaseOrderFact()
    {
        // Three-way match (2026-07-20): this consumer is the received-amount
        // leg's writer — verify it upserts the fact rather than only posting
        // the AP charge and journal entry.
        var ledgerRepository = Substitute.For<IApLedgerRepository>();
        var factRepository = Substitute.For<IPurchaseOrderFactRepository>();
        factRepository.GetByPurchaseOrderIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((PurchaseOrderFact?)null);
        var journalEntryRepository = Substitute.For<IJournalEntryRepository>();
        var settingsRepository = Substitute.For<IFinanceSettingsRepository>();
        settingsRepository.GetByCompanyIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((FusionOS.Modules.Finance.Domain.Settings.FinanceSettings?)null);
        var processedEvents = Substitute.For<IProcessedIntegrationEventStore>();
        processedEvents.HasProcessedAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        var unitOfWork = Substitute.For<IUnitOfWork>();

        var consumer = new PurchaseOrderGoodsReceiptCostedConsumer(ledgerRepository, factRepository, journalEntryRepository, settingsRepository, processedEvents, unitOfWork);
        var purchaseOrderId = Guid.NewGuid();
        var payload = BuildPayload(purchaseOrderId, Guid.NewGuid(), Guid.NewGuid(), 800m);

        await consumer.HandleAsync(Guid.NewGuid(), Guid.NewGuid(), payload, CancellationToken.None);

        await factRepository.Received(1).AddAsync(
            Arg.Is<PurchaseOrderFact>(f => f.PurchaseOrderId == purchaseOrderId && f.ReceivedAmount == 800m && f.OrderedAmount == null),
            Arg.Any<CancellationToken>());
    }

    // Malformed-but-parseable payload guard (2026-07-21): System.Text.Json
    // silently defaults a missing/mistyped property to default(T) instead of
    // throwing, so an event missing PurchaseOrderId/CompanyId/SupplierId (or
    // carrying a negative LineAmount) would otherwise post a real AP charge
    // and PurchaseOrderFact keyed on Guid.Empty / bad data with no error.

    [Fact]
    public async Task HandleAsync_WhenPurchaseOrderIdIsMissing_ThrowsAndDoesNotPersist()
    {
        var ledgerRepository = Substitute.For<IApLedgerRepository>();
        var factRepository = Substitute.For<IPurchaseOrderFactRepository>();
        var journalEntryRepository = Substitute.For<IJournalEntryRepository>();
        var settingsRepository = Substitute.For<IFinanceSettingsRepository>();
        var processedEvents = Substitute.For<IProcessedIntegrationEventStore>();
        processedEvents.HasProcessedAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var consumer = new PurchaseOrderGoodsReceiptCostedConsumer(ledgerRepository, factRepository, journalEntryRepository, settingsRepository, processedEvents, unitOfWork);
        var payload = JsonSerializer.Serialize(new
        {
            CompanyId = Guid.NewGuid(),
            SupplierId = Guid.NewGuid(),
            ProductId = Guid.NewGuid(),
            QuantityReceived = 10m,
            UnitCost = 80m,
            LineAmount = 800m,
        });

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => consumer.HandleAsync(Guid.NewGuid(), Guid.NewGuid(), payload, CancellationToken.None));

        await ledgerRepository.DidNotReceive().AddAsync(Arg.Any<ApLedgerEntry>(), Arg.Any<CancellationToken>());
        await factRepository.DidNotReceive().AddAsync(Arg.Any<PurchaseOrderFact>(), Arg.Any<CancellationToken>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenSupplierIdIsEmpty_ThrowsAndDoesNotPersist()
    {
        var ledgerRepository = Substitute.For<IApLedgerRepository>();
        var factRepository = Substitute.For<IPurchaseOrderFactRepository>();
        var journalEntryRepository = Substitute.For<IJournalEntryRepository>();
        var settingsRepository = Substitute.For<IFinanceSettingsRepository>();
        var processedEvents = Substitute.For<IProcessedIntegrationEventStore>();
        processedEvents.HasProcessedAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var consumer = new PurchaseOrderGoodsReceiptCostedConsumer(ledgerRepository, factRepository, journalEntryRepository, settingsRepository, processedEvents, unitOfWork);
        var payload = BuildPayload(Guid.NewGuid(), Guid.NewGuid(), Guid.Empty, 800m);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => consumer.HandleAsync(Guid.NewGuid(), Guid.NewGuid(), payload, CancellationToken.None));

        await ledgerRepository.DidNotReceive().AddAsync(Arg.Any<ApLedgerEntry>(), Arg.Any<CancellationToken>());
        await factRepository.DidNotReceive().AddAsync(Arg.Any<PurchaseOrderFact>(), Arg.Any<CancellationToken>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_WhenLineAmountIsNegative_ThrowsAndDoesNotPersist()
    {
        var ledgerRepository = Substitute.For<IApLedgerRepository>();
        var factRepository = Substitute.For<IPurchaseOrderFactRepository>();
        var journalEntryRepository = Substitute.For<IJournalEntryRepository>();
        var settingsRepository = Substitute.For<IFinanceSettingsRepository>();
        var processedEvents = Substitute.For<IProcessedIntegrationEventStore>();
        processedEvents.HasProcessedAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var consumer = new PurchaseOrderGoodsReceiptCostedConsumer(ledgerRepository, factRepository, journalEntryRepository, settingsRepository, processedEvents, unitOfWork);
        var payload = BuildPayload(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), -800m);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => consumer.HandleAsync(Guid.NewGuid(), Guid.NewGuid(), payload, CancellationToken.None));

        await ledgerRepository.DidNotReceive().AddAsync(Arg.Any<ApLedgerEntry>(), Arg.Any<CancellationToken>());
        await factRepository.DidNotReceive().AddAsync(Arg.Any<PurchaseOrderFact>(), Arg.Any<CancellationToken>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
