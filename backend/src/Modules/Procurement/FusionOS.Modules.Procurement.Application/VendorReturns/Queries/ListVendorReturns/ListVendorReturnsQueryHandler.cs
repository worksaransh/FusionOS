using FusionOS.Modules.Procurement.Application.VendorReturns.Contracts;
using MediatR;

namespace FusionOS.Modules.Procurement.Application.VendorReturns.Queries.ListVendorReturns;

public sealed class ListVendorReturnsQueryHandler : IRequestHandler<ListVendorReturnsQuery, PagedResult<VendorReturnDto>>
{
    private readonly IVendorReturnRepository _repository;

    public ListVendorReturnsQueryHandler(IVendorReturnRepository repository) => _repository = repository;

    public async Task<PagedResult<VendorReturnDto>> Handle(ListVendorReturnsQuery request, CancellationToken cancellationToken)
    {
        var vendorReturns = await _repository.ListAsync(request.CompanyId, request.PurchaseOrderId, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, request.PurchaseOrderId, cancellationToken);

        var dtos = vendorReturns.Select(VendorReturnMapper.ToDto).ToList();

        return new PagedResult<VendorReturnDto>(dtos, request.Page, request.PageSize, total);
    }
}
