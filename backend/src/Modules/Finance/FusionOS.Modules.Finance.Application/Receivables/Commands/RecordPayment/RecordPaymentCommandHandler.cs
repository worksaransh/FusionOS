using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.Receivables.Contracts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.Receivables.Commands.RecordPayment;

public sealed class RecordPaymentCommandHandler : IRequestHandler<RecordPaymentCommand, ArLedgerEntryDto>
{
    private readonly IArLedgerRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public RecordPaymentCommandHandler(IArLedgerRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ArLedgerEntryDto> Handle(RecordPaymentCommand request, CancellationToken cancellationToken)
    {
        // 2026-07-15 Phase M4: reject a payment that would overpay the invoice
        // (drive its net ledger balance below zero) — same "don't let a
        // transaction exceed what's actually outstanding" ethic as Sales'
        // CreateInvoice/CreateDispatch quantity checks against a Sales Order.
        var outstandingForInvoice = await _repository.SumAmountByInvoiceAsync(request.CompanyId, request.InvoiceId, cancellationToken);
        if (request.Amount > outstandingForInvoice)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(
                    nameof(request.Amount),
                    $"Payment of {request.Amount} exceeds invoice {request.InvoiceId}'s outstanding balance of {outstandingForInvoice}."),
            });
        }

        var entry = Domain.Receivables.ArLedgerEntry.RecordPayment(
            request.CompanyId, request.CustomerId, request.InvoiceId, request.Amount, request.Reference, request.PaymentDate);

        await _repository.AddAsync(entry, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ArLedgerEntryDto(entry.Id, entry.CustomerId, entry.InvoiceId, entry.Amount, entry.Description, entry.TransactionDate);
    }
}
