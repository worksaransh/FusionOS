using FusionOS.Modules.Procurement.Application.Rfqs.Contracts;
using FusionOS.Modules.Procurement.Application.Suppliers.Contracts;
using MediatR;

namespace FusionOS.Modules.Procurement.Application.Rfqs.Queries.GetRfqQuoteComparison;

public sealed class GetRfqQuoteComparisonQueryHandler : IRequestHandler<GetRfqQuoteComparisonQuery, RfqQuoteComparisonDto>
{
    private readonly IRfqRepository _repository;
    private readonly ISupplierRepository _supplierRepository;

    public GetRfqQuoteComparisonQueryHandler(IRfqRepository repository, ISupplierRepository supplierRepository)
    {
        _repository = repository;
        _supplierRepository = supplierRepository;
    }

    public async Task<RfqQuoteComparisonDto> Handle(GetRfqQuoteComparisonQuery request, CancellationToken cancellationToken)
    {
        var rfq = await _repository.GetByIdAsync(request.CompanyId, request.RfqId, cancellationToken)
            ?? throw new KeyNotFoundException($"RFQ '{request.RfqId}' was not found.");

        // Supplier is a real, same-module FK (see SupplierQuote's own doc comment) —
        // safe to resolve names here rather than push that lookup onto the frontend.
        var suppliers = new List<RfqQuoteComparisonSupplierDto>();
        foreach (var quote in rfq.SupplierQuotes)
        {
            var supplier = await _supplierRepository.GetByIdAsync(request.CompanyId, quote.SupplierId, cancellationToken);
            suppliers.Add(new RfqQuoteComparisonSupplierDto(quote.Id, quote.SupplierId, supplier?.Name, quote.TotalAmount));
        }

        var lines = rfq.Lines.Select(line =>
        {
            var cells = rfq.SupplierQuotes
                .Select(quote => (Quote: quote, Line: quote.Lines.FirstOrDefault(l => l.ProductId == line.ProductId)))
                .Where(x => x.Line is not null)
                .Select(x => new RfqQuoteComparisonCellDto(x.Quote.SupplierId, x.Line!.UnitPrice, x.Line!.LineTotal))
                .ToList();

            return new RfqQuoteComparisonLineDto(line.Id, line.ProductId, line.Quantity, cells);
        }).ToList();

        return new RfqQuoteComparisonDto(rfq.Id, rfq.Status.ToString(), suppliers, lines);
    }
}
