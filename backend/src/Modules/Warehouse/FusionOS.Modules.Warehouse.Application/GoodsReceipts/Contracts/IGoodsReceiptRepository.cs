namespace FusionOS.Modules.Warehouse.Application.GoodsReceipts.Contracts;

public interface IGoodsReceiptRepository
{
    /// <summary>
    /// No companyId parameter — same shape as IZoneRepository.GetByIdAsync. Callers
    /// (the Suggest/Confirm Putaway handlers) are responsible for checking the
    /// returned receipt's own CompanyId against the caller's, same convention as
    /// every PickList command handler this phase's precedent (AssignPickList/
    /// RecordPick/PackPickList) established.
    /// </summary>
    Task<Domain.GoodsReceipts.GoodsReceipt?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ZoneExistsAsync(Guid companyId, Guid warehouseId, Guid zoneId, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.GoodsReceipts.GoodsReceipt receipt, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.GoodsReceipts.GoodsReceipt>> ListAsync(Guid companyId, Guid warehouseId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, Guid warehouseId, CancellationToken cancellationToken = default);
}
