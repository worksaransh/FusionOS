using FusionOS.Modules.Sales.Application.EWayBills.Contracts;
using FusionOS.Modules.Sales.Domain.EWayBills;
using FusionOS.Modules.Sales.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Sales.Infrastructure.Repositories;

public sealed class EWayBillRepository : IEWayBillRepository
{
    private readonly SalesDbContext _context;

    public EWayBillRepository(SalesDbContext context) => _context = context;

    public Task<EWayBill?> GetByDispatchIdAsync(Guid companyId, Guid dispatchId, CancellationToken cancellationToken = default) =>
        _context.EWayBills.FirstOrDefaultAsync(e => e.CompanyId == companyId && e.DispatchId == dispatchId, cancellationToken);

    public async Task AddAsync(EWayBill eWayBill, CancellationToken cancellationToken = default) =>
        await _context.EWayBills.AddAsync(eWayBill, cancellationToken);
}
