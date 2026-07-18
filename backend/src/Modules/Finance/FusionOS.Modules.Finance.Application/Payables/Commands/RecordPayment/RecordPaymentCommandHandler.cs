using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.Payables.Contracts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.Payables.Commands.RecordPayment;

public sealed class RecordPaymentCommandHandler : IRequestHandler<RecordPaymentCommand, ApLedgerEntryDto>
{
    private readonly IApLedgerRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public RecordPaymentCommandHandler(IApLedgerRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ApLedgerEntryDto> Handle(RecordPaymentCommand request, CancellationToken cancellationToken)
    {
        // Reject a payment that would overpay the supplier (drive its net
        // ledger balance below zero) — scoped to the supplier's total
        // outstanding balance rather than one invoice/PO, since
        // PurchaseOrderId is optional here (see ApLedgerEntry's class doc
        // comment). Same "don't let a transaction exceed what's actually
        // outstanding" ethic as AR's RecordPaymentCommandHandler.
        var outstandingForSupplier = await _repository.SumAmountAsync(request.CompanyId, request.SupplierId, cancellationToken);
        if (request.Amount > outstandingForSupplier)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(
                    nameof(request.Amount),
                    $"Payment of {request.Amount} exceeds supplier {request.SupplierId}'s outstanding balance of {outstandingForSupplier}."),
            });
        }

        var entry = Domain.Payables.ApLedgerEntry.RecordPayment(
            request.CompanyId, request.SupplierId, request.PurchaseOrderId, request.Amount, request.Reference, request.PaymentDate);

        await _repository.AddAsync(entry, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ApLedgerEntryDto(entry.Id, entry.SupplierId, entry.PurchaseOrderId, entry.Amount, entry.Description, entry.TransactionDate);
    }
}
