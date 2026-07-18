using FusionOS.Modules.Procurement.Application.Rfqs.Contracts;
using FusionOS.Modules.Procurement.Application.Suppliers.Contracts;
using MediatR;

namespace FusionOS.Modules.Procurement.Application.Rfqs.Commands.CreateRfq;

public sealed class CreateRfqCommandHandler : IRequestHandler<CreateRfqCommand, RfqDto>
{
    private readonly IRfqRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateRfqCommandHandler(IRfqRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<RfqDto> Handle(CreateRfqCommand request, CancellationToken cancellationToken)
    {
        var rfq = Domain.Rfqs.RequestForQuotation.Create(request.CompanyId, request.Lines);

        await _repository.AddAsync(rfq, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(rfq);
    }

    internal static RfqDto MapToDto(Domain.Rfqs.RequestForQuotation rfq) => new(
        rfq.Id,
        rfq.Status.ToString(),
        rfq.RfqDate,
        rfq.AwardedSupplierQuoteId,
        rfq.ConvertedPurchaseOrderId,
        rfq.Lines.Select(l => new RfqLineDto(l.Id, l.ProductId, l.Quantity)).ToList(),
        rfq.SupplierQuotes.Select(q => new SupplierQuoteDto(
            q.Id,
            q.SupplierId,
            q.SubmittedAt,
            q.TotalAmount,
            q.Lines.Select(l => new SupplierQuoteLineDto(l.Id, l.ProductId, l.Quantity, l.UnitPrice, l.LineTotal)).ToList())).ToList());
}
