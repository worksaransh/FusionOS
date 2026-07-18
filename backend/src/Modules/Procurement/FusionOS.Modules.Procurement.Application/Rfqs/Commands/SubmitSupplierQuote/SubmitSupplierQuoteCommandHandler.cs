using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Procurement.Application.Rfqs.Commands.CreateRfq;
using FusionOS.Modules.Procurement.Application.Rfqs.Contracts;
using FusionOS.Modules.Procurement.Application.Suppliers.Contracts;
using MediatR;

namespace FusionOS.Modules.Procurement.Application.Rfqs.Commands.SubmitSupplierQuote;

public sealed class SubmitSupplierQuoteCommandHandler : IRequestHandler<SubmitSupplierQuoteCommand, RfqDto>
{
    private readonly IRfqRepository _repository;
    private readonly ISupplierRepository _supplierRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SubmitSupplierQuoteCommandHandler(IRfqRepository repository, ISupplierRepository supplierRepository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _supplierRepository = supplierRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<RfqDto> Handle(SubmitSupplierQuoteCommand request, CancellationToken cancellationToken)
    {
        var rfq = await _repository.GetByIdAsync(request.CompanyId, request.RfqId, cancellationToken)
            ?? throw new KeyNotFoundException($"RFQ '{request.RfqId}' was not found.");

        if (!await _supplierRepository.ExistsAsync(request.CompanyId, request.SupplierId, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.SupplierId), "Supplier does not exist for this company."),
            });
        }

        rfq.SubmitSupplierQuote(request.SupplierId, request.Lines);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateRfqCommandHandler.MapToDto(rfq);
    }
}
