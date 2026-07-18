using FusionOS.Modules.Sales.Domain.Quotations;
using FluentAssertions;
using Xunit;

namespace FusionOS.Modules.Sales.Tests.Quotations;

public class QuotationTests
{
    private static readonly QuotationLineInput[] OneLine =
    {
        new(Guid.NewGuid(), 3m, 100m),
    };

    [Fact]
    public void Create_WithValidLines_ComputesTotalAmount()
    {
        var quotation = Quotation.Create(Guid.NewGuid(), Guid.NewGuid(), OneLine);

        quotation.Status.Should().Be(QuotationStatus.Draft);
        quotation.TotalAmount.Should().Be(300m);
        quotation.DomainEvents.Should().ContainSingle(e => e is Events.QuotationCreated);
    }

    [Fact]
    public void Create_WithNoLines_Throws()
    {
        var act = () => Quotation.Create(Guid.NewGuid(), Guid.NewGuid(), Array.Empty<QuotationLineInput>());

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Accept_FromDraft_TransitionsToAcceptedAndRaisesEvent()
    {
        var quotation = Quotation.Create(Guid.NewGuid(), Guid.NewGuid(), OneLine);
        quotation.ClearDomainEvents();

        quotation.Accept();

        quotation.Status.Should().Be(QuotationStatus.Accepted);
        quotation.DomainEvents.Should().ContainSingle(e => e is Events.QuotationAccepted);
    }

    [Fact]
    public void Accept_WhenAlreadyAccepted_Throws()
    {
        var quotation = Quotation.Create(Guid.NewGuid(), Guid.NewGuid(), OneLine);
        quotation.Accept();

        var act = () => quotation.Accept();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Reject_FromDraft_TransitionsToRejectedAndRaisesEvent()
    {
        var quotation = Quotation.Create(Guid.NewGuid(), Guid.NewGuid(), OneLine);
        quotation.ClearDomainEvents();

        quotation.Reject();

        quotation.Status.Should().Be(QuotationStatus.Rejected);
        quotation.DomainEvents.Should().ContainSingle(e => e is Events.QuotationRejected);
    }

    [Fact]
    public void Reject_WhenAlreadyAccepted_Throws()
    {
        var quotation = Quotation.Create(Guid.NewGuid(), Guid.NewGuid(), OneLine);
        quotation.Accept();

        var act = () => quotation.Reject();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void MarkConverted_FromAccepted_TransitionsToConvertedAndRaisesEvent()
    {
        var quotation = Quotation.Create(Guid.NewGuid(), Guid.NewGuid(), OneLine);
        quotation.Accept();
        quotation.ClearDomainEvents();
        var salesOrderId = Guid.NewGuid();

        quotation.MarkConverted(salesOrderId);

        quotation.Status.Should().Be(QuotationStatus.Converted);
        quotation.ConvertedSalesOrderId.Should().Be(salesOrderId);
        quotation.DomainEvents.Should().ContainSingle(e => e is Events.QuotationConverted c && c.SalesOrderId == salesOrderId);
    }

    [Fact]
    public void MarkConverted_WhenStillDraft_Throws()
    {
        var quotation = Quotation.Create(Guid.NewGuid(), Guid.NewGuid(), OneLine);

        var act = () => quotation.MarkConverted(Guid.NewGuid());

        act.Should().Throw<InvalidOperationException>();
    }
}
