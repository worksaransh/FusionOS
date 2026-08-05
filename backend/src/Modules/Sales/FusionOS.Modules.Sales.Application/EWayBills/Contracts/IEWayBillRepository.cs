namespace FusionOS.Modules.Sales.Application.EWayBills.Contracts;

public interface IEWayBillRepository
{
    Task<Domain.EWayBills.EWayBill?> GetByDispatchIdAsync(Guid companyId, Guid dispatchId, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.EWayBills.EWayBill eWayBill, CancellationToken cancellationToken = default);
}
