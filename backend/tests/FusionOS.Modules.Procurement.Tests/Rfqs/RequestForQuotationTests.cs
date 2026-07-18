using FusionOS.Modules.Procurement.Domain.Rfqs;
using FluentAssertions;
using Xunit;

namespace FusionOS.Modules.Procurement.Tests.Rfqs;

public class RequestForQuotationTests
{
    private static RfqLineInput[] OneLine(Guid productId) => new[] { new RfqLineInput(productId, 10m) };

    [Fact]
    public void Create_WithValidLines_StartsInDraft()
    {
        var productId = Guid.NewGuid();
        var rfq = RequestForQuotation.Create(Guid.NewGuid(), OneLine(productId));

        rfq.Status.Should().Be(RfqStatus.Draft);
        rfq.Lines.Should().ContainSingle(l => l.ProductId == productId && l.Quantity == 10m);
        rfq.DomainEvents.Should().ContainSingle(e => e is Events.RfqCreated);
    }

    [Fact]
    public void Create_WithNoLines_Throws()
    {
        var act = () => RequestForQuotation.Create(Guid.NewGuid(), Array.Empty<RfqLineInput>());

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Send_FromDraft_TransitionsToSentAndRaisesEvent()
    {
        var rfq = RequestForQuotation.Create(Guid.NewGuid(), OneLine(Guid.NewGuid()));
        rfq.ClearDomainEvents();

        rfq.Send();

        rfq.Status.Should().Be(RfqStatus.Sent);
        rfq.DomainEvents.Should().ContainSingle(e => e is Events.RfqSent);
    }

    [Fact]
    public void Send_WhenAlreadySent_Throws()
    {
        var rfq = RequestForQuotation.Create(Guid.NewGuid(), OneLine(Guid.NewGuid()));
        rfq.Send();

        var act = () => rfq.Send();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void SubmitSupplierQuote_WhileSent_AddsQuoteAndComputesTotal()
    {
        var productId = Guid.NewGuid();
        var rfq = RequestForQuotation.Create(Guid.NewGuid(), OneLine(productId));
        rfq.Send();
        rfq.ClearDomainEvents();
        var supplierId = Guid.NewGuid();

        var quote = rfq.SubmitSupplierQuote(supplierId, new[] { new SupplierQuoteLineInput(productId, 10m, 5m) });

        quote.TotalAmount.Should().Be(50m);
        rfq.SupplierQuotes.Should().ContainSingle(q => q.Id == quote.Id && q.SupplierId == supplierId);
        rfq.DomainEvents.Should().ContainSingle(e => e is Events.SupplierQuoteSubmitted);
    }

    [Fact]
    public void SubmitSupplierQuote_WhileStillDraft_Throws()
    {
        var productId = Guid.NewGuid();
        var rfq = RequestForQuotation.Create(Guid.NewGuid(), OneLine(productId));

        var act = () => rfq.SubmitSupplierQuote(Guid.NewGuid(), new[] { new SupplierQuoteLineInput(productId, 10m, 5m) });

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void SubmitSupplierQuote_SameSupplierTwice_ReplacesPriorQuote()
    {
        var productId = Guid.NewGuid();
        var rfq = RequestForQuotation.Create(Guid.NewGuid(), OneLine(productId));
        rfq.Send();
        var supplierId = Guid.NewGuid();
        rfq.SubmitSupplierQuote(supplierId, new[] { new SupplierQuoteLineInput(productId, 10m, 5m) });

        var replacement = rfq.SubmitSupplierQuote(supplierId, new[] { new SupplierQuoteLineInput(productId, 10m, 6m) });

        // The supplier still has exactly one quote on the RFQ — the resubmission replaced the first.
        rfq.SupplierQuotes.Should().ContainSingle(q => q.SupplierId == supplierId);
        rfq.SupplierQuotes.Single(q => q.SupplierId == supplierId).Id.Should().Be(replacement.Id);
        replacement.TotalAmount.Should().Be(60m); // 10 * 6
    }

    [Fact]
    public void SubmitSupplierQuote_ForProductNotOnRfq_Throws()
    {
        var rfq = RequestForQuotation.Create(Guid.NewGuid(), OneLine(Guid.NewGuid()));
        rfq.Send();

        var act = () => rfq.SubmitSupplierQuote(Guid.NewGuid(), new[] { new SupplierQuoteLineInput(Guid.NewGuid(), 10m, 5m) });

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Award_WithSubmittedQuote_TransitionsToAwardedAndRaisesEvent()
    {
        var productId = Guid.NewGuid();
        var rfq = RequestForQuotation.Create(Guid.NewGuid(), OneLine(productId));
        rfq.Send();
        var supplierId = Guid.NewGuid();
        var quote = rfq.SubmitSupplierQuote(supplierId, new[] { new SupplierQuoteLineInput(productId, 10m, 5m) });
        rfq.ClearDomainEvents();

        rfq.Award(quote.Id);

        rfq.Status.Should().Be(RfqStatus.Awarded);
        rfq.AwardedSupplierQuoteId.Should().Be(quote.Id);
        rfq.DomainEvents.Should().ContainSingle(e => e is Events.RfqAwarded a && a.SupplierId == supplierId);
    }

    [Fact]
    public void Award_WithUnknownQuoteId_Throws()
    {
        var rfq = RequestForQuotation.Create(Guid.NewGuid(), OneLine(Guid.NewGuid()));
        rfq.Send();

        var act = () => rfq.Award(Guid.NewGuid());

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void MarkConverted_FromAwarded_TransitionsAndRaisesEvent()
    {
        var productId = Guid.NewGuid();
        var rfq = RequestForQuotation.Create(Guid.NewGuid(), OneLine(productId));
        rfq.Send();
        var quote = rfq.SubmitSupplierQuote(Guid.NewGuid(), new[] { new SupplierQuoteLineInput(productId, 10m, 5m) });
        rfq.Award(quote.Id);
        rfq.ClearDomainEvents();
        var purchaseOrderId = Guid.NewGuid();

        rfq.MarkConverted(purchaseOrderId);

        rfq.ConvertedPurchaseOrderId.Should().Be(purchaseOrderId);
        rfq.DomainEvents.Should().ContainSingle(e => e is Events.RfqConverted c && c.PurchaseOrderId == purchaseOrderId);
    }

    [Fact]
    public void MarkConverted_WhileStillSent_Throws()
    {
        var rfq = RequestForQuotation.Create(Guid.NewGuid(), OneLine(Guid.NewGuid()));
        rfq.Send();

        var act = () => rfq.MarkConverted(Guid.NewGuid());

        act.Should().Throw<InvalidOperationException>();
    }
}
