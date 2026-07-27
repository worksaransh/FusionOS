using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Procurement.Application.SupplierRatings.Commands.CreateSupplierRating;
using FusionOS.Modules.Procurement.Application.SupplierRatings.Contracts;
using MediatR;

namespace FusionOS.Modules.Procurement.Application.SupplierRatings.Queries.ListSupplierRatingsBySupplier;

public sealed class ListSupplierRatingsBySupplierQueryHandler : IRequestHandler<ListSupplierRatingsBySupplierQuery, PagedResult<SupplierRatingDto>>
{
    private readonly ISupplierRatingRepository _repository;

    public ListSupplierRatingsBySupplierQueryHandler(ISupplierRatingRepository repository) => _repository = repository;

    public async Task<PagedResult<SupplierRatingDto>> Handle(ListSupplierRatingsBySupplierQuery request, CancellationToken cancellationToken)
    {
        var ratings = await _repository.ListBySupplierAsync(request.CompanyId, request.SupplierId, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountBySupplierAsync(request.CompanyId, request.SupplierId, cancellationToken);

        var dtos = ratings.Select(CreateSupplierRatingCommandHandler.MapToDto).ToList();

        return new PagedResult<SupplierRatingDto>(dtos, request.Page, request.PageSize, total);
    }
}
