using FusionOS.Modules.Finance.Application.Payables.Contracts;
using FusionOS.Modules.Finance.Domain.Payables;
using FusionOS.Modules.Finance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Finance.Infrastructure.Repositories;

public sealed class PurchaseOrderFactRepository : IPurchaseOrderFactRepository
{
    private readonly FinanceDbContext _context;

    public PurchaseOrderFactRepository(FinanceDbContext context) => _context = context;

    public Task<PurchaseOrderFact?> GetByPurchaseOrderIdAsync(Guid companyId, Guid purchaseOrderId, CancellationToken cancellationToken = default) =>
        _context.PurchaseOrderFacts
            .FirstOrDefaultAsync(x => x.CompanyId == companyId && x.PurchaseOrderId == purchaseOrderId, cancellationToken);

    public async Task AddAsync(PurchaseOrderFact fact, CancellationToken cancellationToken = default) =>
        await _context.PurchaseOrderFacts.AddAsync(fact, cancellationToken);
}
