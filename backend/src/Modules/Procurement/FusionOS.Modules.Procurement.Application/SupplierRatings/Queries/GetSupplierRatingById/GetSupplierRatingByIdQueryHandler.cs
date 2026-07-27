using FusionOS.Modules.Procurement.Application.SupplierRatings.Commands.CreateSupplierRating;
using FusionOS.Modules.Procurement.Application.SupplierRatings.Contracts;
using MediatR;

namespace FusionOS.Modules.Procurement.Application.SupplierRatings.Queries.GetSupplierRatingById;

public sealed class GetSupplierRatingByIdQueryHandler : IRequestHandler<GetSupplierRatingByIdQuery, SupplierRatingDto>
{
    private readonly ISupplierRatingRepository _repository;

    public GetSupplierRatingByIdQueryHandler(ISupplierRatingRepository repository) => _repository = repository;

    public async Task<SupplierRatingDto> Handle(GetSupplierRatingByIdQuery request, CancellationToken cancellationToken)
    {
        var rating = await _repository.GetByIdAsync(request.CompanyId, request.SupplierRatingId, cancellationToken)
            ?? throw new KeyNotFoundException($"Supplier rating '{request.SupplierRatingId}' was not found.");

        return CreateSupplierRatingCommandHandler.MapToDto(rating);
    }
}
