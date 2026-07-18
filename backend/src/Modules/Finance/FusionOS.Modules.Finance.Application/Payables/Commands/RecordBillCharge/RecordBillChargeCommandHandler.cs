using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.Payables.Contracts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.Payables.Commands.RecordBillCharge;

public sealed class RecordBillChargeCommandHandler : IRequestHandler<RecordBillChargeCommand, ApLedgerEntryDto>
{
    private readonly IApLedgerRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public RecordBillChargeCommandHandler(IApLedgerRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ApLedgerEntryDto> Handle(RecordBillChargeCommand request, CancellationToken cancellationToken)
    {
        var entry = Domain.Payables.ApLedgerEntry.RecordBillCharge(
            request.CompanyId, request.SupplierId, request.PurchaseOrderId, request.Amount, request.Description);

        await _repository.AddAsync(entry, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ApLedgerEntryDto(entry.Id, entry.SupplierId, entry.PurchaseOrderId, entry.Amount, entry.Description, entry.TransactionDate);
    }
}
