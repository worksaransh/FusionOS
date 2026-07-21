namespace FusionOS.Modules.Finance.Application.Payables.Contracts;

/// <summary>
/// Access to Finance's local purchase-order fact read-model (see
/// PurchaseOrderFact's class doc comment for what it is and the three-way-match
/// policy it backs). Deliberately tiny: consumers upsert one fact at a time and
/// RecordBillChargeCommandHandler reads one fact at a time — there is no list
/// or reporting surface over these rows in this slice.
/// </summary>
public interface IPurchaseOrderFactRepository
{
    Task<Domain.Payables.PurchaseOrderFact?> GetByPurchaseOrderIdAsync(Guid companyId, Guid purchaseOrderId, CancellationToken cancellationToken = default);

    Task AddAsync(Domain.Payables.PurchaseOrderFact fact, CancellationToken cancellationToken = default);
}
