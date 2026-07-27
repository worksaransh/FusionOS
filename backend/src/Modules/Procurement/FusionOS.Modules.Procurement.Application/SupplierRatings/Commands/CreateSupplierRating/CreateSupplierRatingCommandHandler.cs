using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Procurement.Application.PurchaseOrders.Contracts;
using FusionOS.Modules.Procurement.Application.SupplierRatings.Contracts;
using FusionOS.Modules.Procurement.Application.Suppliers.Contracts;
using FusionOS.Modules.Procurement.Domain.PurchaseOrders;
using MediatR;

namespace FusionOS.Modules.Procurement.Application.SupplierRatings.Commands.CreateSupplierRating;

public sealed class CreateSupplierRatingCommandHandler : IRequestHandler<CreateSupplierRatingCommand, SupplierRatingDto>
{
    private readonly ISupplierRatingRepository _repository;
    private readonly ISupplierRepository _supplierRepository;
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateSupplierRatingCommandHandler(
        ISupplierRatingRepository repository,
        ISupplierRepository supplierRepository,
        IPurchaseOrderRepository purchaseOrderRepository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _supplierRepository = supplierRepository;
        _purchaseOrderRepository = purchaseOrderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<SupplierRatingDto> Handle(CreateSupplierRatingCommand request, CancellationToken cancellationToken)
    {
        if (!await _supplierRepository.ExistsAsync(request.CompanyId, request.SupplierId, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.SupplierId), "Supplier does not exist for this company."),
            });
        }

        // A rating tied to a specific PO only makes sense once that PO actually
        // delivered — this is a command-handler-level rule (not a domain
        // invariant), since it depends on cross-aggregate state (PurchaseOrder's
        // own SupplierId and Status), same layering as the FK check above.
        if (request.PurchaseOrderId.HasValue)
        {
            var purchaseOrder = await _purchaseOrderRepository.GetByIdAsync(request.CompanyId, request.PurchaseOrderId.Value, cancellationToken);
            if (purchaseOrder is null)
            {
                throw new ValidationException(new[]
                {
                    new FluentValidation.Results.ValidationFailure(nameof(request.PurchaseOrderId), "Purchase order does not exist for this company."),
                });
            }

            if (purchaseOrder.SupplierId != request.SupplierId)
            {
                throw new ValidationException(new[]
                {
                    new FluentValidation.Results.ValidationFailure(nameof(request.PurchaseOrderId), "Purchase order does not belong to the specified supplier."),
                });
            }

            if (purchaseOrder.Status != PurchaseOrderStatus.FullyReceived)
            {
                throw new ValidationException(new[]
                {
                    new FluentValidation.Results.ValidationFailure(nameof(request.PurchaseOrderId), "Only a fully received purchase order can be rated."),
                });
            }
        }

        var rating = Domain.SupplierRatings.SupplierRating.Create(
            request.CompanyId,
            request.SupplierId,
            request.PurchaseOrderId,
            request.QualityScore,
            request.DeliveryScore,
            request.RatedByUserId,
            request.Comments);

        await _repository.AddAsync(rating, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(rating);
    }

    internal static SupplierRatingDto MapToDto(Domain.SupplierRatings.SupplierRating rating) => new(
        rating.Id,
        rating.SupplierId,
        rating.PurchaseOrderId,
        rating.QualityScore,
        rating.DeliveryScore,
        rating.Comments,
        rating.RatedByUserId,
        rating.RatedAt);
}
