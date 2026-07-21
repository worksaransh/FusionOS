using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.Payables.Contracts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.Payables.Commands.RecordBillCharge;

public sealed class RecordBillChargeCommandHandler : IRequestHandler<RecordBillChargeCommand, ApLedgerEntryDto>
{
    private readonly IApLedgerRepository _repository;
    private readonly IPurchaseOrderFactRepository _factRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RecordBillChargeCommandHandler(IApLedgerRepository repository, IPurchaseOrderFactRepository factRepository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _factRepository = factRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ApLedgerEntryDto> Handle(RecordBillChargeCommand request, CancellationToken cancellationToken)
    {
        // Three-way match (2026-07-20) - only applies when this manual charge
        // names a PurchaseOrderId; an ad-hoc bill with none has nothing to
        // match against and is accepted exactly as before. See
        // PurchaseOrderFact's class doc comment for the full eventual-
        // consistency policy this enforces (both legs are "skip if we don't
        // have the fact yet," never a hard rejection due to Kafka lag).
        if (request.PurchaseOrderId is { } purchaseOrderId)
        {
            var fact = await _factRepository.GetByPurchaseOrderIdAsync(request.CompanyId, purchaseOrderId, cancellationToken);
            if (fact is not null)
            {
                var alreadyCharged = await _repository.SumAmountByPurchaseOrderAsync(request.CompanyId, purchaseOrderId, cancellationToken);
                var totalAfterThisCharge = alreadyCharged + request.Amount;
                var failures = new List<FluentValidation.Results.ValidationFailure>();

                if (fact.OrderedAmount is { } orderedAmount && totalAfterThisCharge > orderedAmount)
                {
                    failures.Add(new FluentValidation.Results.ValidationFailure(
                        nameof(request.Amount),
                        $"Charging {request.Amount} against purchase order {purchaseOrderId} would bring the total billed to " +
                        $"{totalAfterThisCharge}, exceeding the ordered amount of {orderedAmount} ({alreadyCharged} already billed)."));
                }

                if (fact.ReceivedAmount > 0m && totalAfterThisCharge > fact.ReceivedAmount)
                {
                    failures.Add(new FluentValidation.Results.ValidationFailure(
                        nameof(request.Amount),
                        $"Charging {request.Amount} against purchase order {purchaseOrderId} would bring the total billed to " +
                        $"{totalAfterThisCharge}, exceeding the {fact.ReceivedAmount} received so far ({alreadyCharged} already billed)."));
                }

                if (failures.Count > 0)
                    throw new ValidationException(failures);
            }
            // fact is null: Finance has learned nothing about this purchase
            // order yet (events predate these consumers, or haven't arrived) -
            // accepted unvalidated, exactly the pre-three-way-match behavior.
        }

        var entry = Domain.Payables.ApLedgerEntry.RecordBillCharge(
            request.CompanyId, request.SupplierId, request.PurchaseOrderId, request.Amount, request.Description);

        await _repository.AddAsync(entry, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ApLedgerEntryDto(entry.Id, entry.SupplierId, entry.PurchaseOrderId, entry.Amount, entry.Description, entry.TransactionDate);
    }
}
