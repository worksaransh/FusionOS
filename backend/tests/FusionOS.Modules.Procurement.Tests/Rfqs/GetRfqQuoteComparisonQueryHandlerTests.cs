using FusionOS.Modules.Procurement.Application.Rfqs.Contracts;
using FusionOS.Modules.Procurement.Application.Rfqs.Queries.GetRfqQuoteComparison;
using FusionOS.Modules.Procurement.Application.Suppliers.Contracts;
using FusionOS.Modules.Procurement.Domain.Rfqs;
using FusionOS.Modules.Procurement.Domain.Suppliers;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Procurement.Tests.Rfqs;

public class GetRfqQuoteComparisonQueryHandlerTests
{
    [Fact]
    public async Task Handle_WithTwoSuppliersOnOverlappingAndDisjointLines_BuildsComparisonShape()
    {
        var companyId = Guid.NewGuid();
        var productA = Guid.NewGuid();
        var productB = Guid.NewGuid();

        // Product A is on both quotes (overlapping), product B only on supplier 1's quote.
        var rfq = RequestForQuotation.Create(companyId, new[]
        {
            new RfqLineInput(productA, 10m),
            new RfqLineInput(productB, 5m),
        });
        rfq.Send();

        var supplier1Id = Guid.NewGuid();
        var supplier2Id = Guid.NewGuid();
        var quote1 = rfq.SubmitSupplierQuote(supplier1Id, new[]
        {
            new SupplierQuoteLineInput(productA, 10m, 5m),
            new SupplierQuoteLineInput(productB, 5m, 2m),
        });
        var quote2 = rfq.SubmitSupplierQuote(supplier2Id, new[]
        {
            new SupplierQuoteLineInput(productA, 10m, 4m),
        });

        var repository = Substitute.For<IRfqRepository>();
        repository.GetByIdAsync(companyId, rfq.Id, Arg.Any<CancellationToken>()).Returns(rfq);

        var supplier1 = Supplier.Create(companyId, "Acme Supplies", "ACME");
        var supplier2 = Supplier.Create(companyId, "Beta Traders", "BETA");
        var supplierRepository = Substitute.For<ISupplierRepository>();
        supplierRepository.GetByIdAsync(companyId, supplier1Id, Arg.Any<CancellationToken>()).Returns(supplier1);
        supplierRepository.GetByIdAsync(companyId, supplier2Id, Arg.Any<CancellationToken>()).Returns(supplier2);

        var handler = new GetRfqQuoteComparisonQueryHandler(repository, supplierRepository);

        var result = await handler.Handle(new GetRfqQuoteComparisonQuery(companyId, rfq.Id), CancellationToken.None);

        result.RfqId.Should().Be(rfq.Id);
        result.Suppliers.Should().HaveCount(2);
        result.Suppliers.Should().ContainSingle(s => s.SupplierQuoteId == quote1.Id && s.SupplierName == "Acme Supplies" && s.TotalAmount == 60m);
        result.Suppliers.Should().ContainSingle(s => s.SupplierQuoteId == quote2.Id && s.SupplierName == "Beta Traders" && s.TotalAmount == 40m);

        result.Lines.Should().HaveCount(2);

        var lineA = result.Lines.Single(l => l.ProductId == productA);
        lineA.Quotes.Should().HaveCount(2);
        lineA.Quotes.Should().ContainSingle(c => c.SupplierId == supplier1Id && c.UnitPrice == 5m && c.LineTotal == 50m);
        lineA.Quotes.Should().ContainSingle(c => c.SupplierId == supplier2Id && c.UnitPrice == 4m && c.LineTotal == 40m);

        // Product B was only quoted by supplier 1 — supplier 2 must be absent, not a null-filled cell.
        var lineB = result.Lines.Single(l => l.ProductId == productB);
        lineB.Quotes.Should().ContainSingle(c => c.SupplierId == supplier1Id && c.UnitPrice == 2m && c.LineTotal == 10m);
        lineB.Quotes.Should().NotContain(c => c.SupplierId == supplier2Id);
    }

    [Fact]
    public async Task Handle_WhenSupplierNoLongerResolvable_LeavesSupplierNameNull()
    {
        var companyId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var rfq = RequestForQuotation.Create(companyId, new[] { new RfqLineInput(productId, 10m) });
        rfq.Send();
        var supplierId = Guid.NewGuid();
        rfq.SubmitSupplierQuote(supplierId, new[] { new SupplierQuoteLineInput(productId, 10m, 3m) });

        var repository = Substitute.For<IRfqRepository>();
        repository.GetByIdAsync(companyId, rfq.Id, Arg.Any<CancellationToken>()).Returns(rfq);
        var supplierRepository = Substitute.For<ISupplierRepository>();
        supplierRepository.GetByIdAsync(companyId, supplierId, Arg.Any<CancellationToken>()).Returns((Supplier?)null);

        var handler = new GetRfqQuoteComparisonQueryHandler(repository, supplierRepository);

        var result = await handler.Handle(new GetRfqQuoteComparisonQuery(companyId, rfq.Id), CancellationToken.None);

        result.Suppliers.Should().ContainSingle(s => s.SupplierId == supplierId && s.SupplierName == null);
    }

    [Fact]
    public async Task Handle_WhenRfqDoesNotExist_ThrowsKeyNotFoundException()
    {
        var companyId = Guid.NewGuid();
        var rfqId = Guid.NewGuid();
        var repository = Substitute.For<IRfqRepository>();
        repository.GetByIdAsync(companyId, rfqId, Arg.Any<CancellationToken>()).Returns((RequestForQuotation?)null);
        var supplierRepository = Substitute.For<ISupplierRepository>();
        var handler = new GetRfqQuoteComparisonQueryHandler(repository, supplierRepository);

        var act = () => handler.Handle(new GetRfqQuoteComparisonQuery(companyId, rfqId), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
