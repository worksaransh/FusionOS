namespace FusionOS.Modules.Inventory.Application.Transfers.Contracts;

public interface ITransferRepository
{
    Task<Domain.Transfers.Transfer?> GetByIdAsync(Guid companyId, Guid transferId, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.Transfers.Transfer transfer, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Transfers.Transfer>> ListAsync(Guid companyId, Guid? productId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, Guid? productId, CancellationToken cancellationToken = default);
}
