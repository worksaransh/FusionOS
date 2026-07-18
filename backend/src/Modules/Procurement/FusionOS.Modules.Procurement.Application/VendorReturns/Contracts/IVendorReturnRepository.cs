namespace FusionOS.Modules.Procurement.Application.VendorReturns.Contracts;

public interface IVendorReturnRepository
{
    Task<Domain.VendorReturns.VendorReturn?> GetByIdAsync(Guid companyId, Guid vendorReturnId, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.VendorReturns.VendorReturn vendorReturn, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.VendorReturns.VendorReturn>> ListAsync(Guid companyId, Guid? purchaseOrderId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, Guid? purchaseOrderId, CancellationToken cancellationToken = default);

    /// <summary>Sum of every non-Cancelled return's Quantity for this PurchaseOrder/Product — the "already returned" half of CreateVendorReturnCommandHandler's over-return guard.</summary>
    Task<decimal> SumReturnedQuantityAsync(Guid companyId, Guid purchaseOrderId, Guid productId, CancellationToken cancellationToken = default);
}
