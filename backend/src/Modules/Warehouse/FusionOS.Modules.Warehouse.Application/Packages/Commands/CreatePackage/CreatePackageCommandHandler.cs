using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Warehouse.Application.Packages.Contracts;
using FusionOS.Modules.Warehouse.Application.PickLists.Contracts;
using FusionOS.Modules.Warehouse.Domain.PickLists;
using MediatR;

namespace FusionOS.Modules.Warehouse.Application.Packages.Commands.CreatePackage;

public sealed class CreatePackageCommandHandler : IRequestHandler<CreatePackageCommand, PackageDto>
{
    private readonly IPackageRepository _repository;
    private readonly IPickListRepository _pickListRepository;
    private readonly FusionOS.Modules.Warehouse.Application.Warehouses.Contracts.IUnitOfWork _unitOfWork;

    public CreatePackageCommandHandler(
        IPackageRepository repository,
        IPickListRepository pickListRepository,
        FusionOS.Modules.Warehouse.Application.Warehouses.Contracts.IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _pickListRepository = pickListRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PackageDto> Handle(CreatePackageCommand request, CancellationToken cancellationToken)
    {
        var pickList = await _pickListRepository.GetByIdAsync(request.PickListId, cancellationToken);
        if (pickList is null || pickList.CompanyId != request.CompanyId)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.PickListId), "Pick list does not exist for this company."),
            });
        }

        // A package can only be recorded once picking+packing confirmation has actually happened —
        // PickList.Pack()'s own precondition (every line fully picked) already guarantees that by
        // the time Status reaches Packed, so this is just re-checking the same invariant from the
        // Package side rather than re-deriving it.
        if (pickList.Status != PickListStatus.Packed)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(
                    nameof(request.PickListId),
                    $"Only a fully-packed pick list can have packages recorded against it (current status: {pickList.Status})."),
            });
        }

        if (await _repository.PackageNumberExistsAsync(request.CompanyId, request.PickListId, request.PackageNumber, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.PackageNumber), $"Package number '{request.PackageNumber}' already exists on this pick list."),
            });
        }

        // Cross-aggregate quantity guard (2026-07-21 packing depth pass): the cumulative packaged
        // quantity per product - every existing Package on this pick list plus every line of this
        // request - must never exceed the quantity actually picked for that product on the pick
        // list (PickListLine.QuantityPicked), mirroring CreateInvoiceCommandHandler's/
        // CreateDispatchCommandHandler's ordered/dispatched-quantity guards (Sales) exactly.
        //
        // Counting rule: every persisted Package counts toward the cap - a Package has no
        // status/lifecycle (no cancelled/void state), so each line is goods already sealed into a
        // physical carton against this pick list.
        //
        // Request lines are grouped by product before checking, so the same product split across
        // several request lines cannot slip past the cap by each line passing individually; the cap
        // itself sums every pick list line carrying the product, in case the same product appears on
        // more than one pick list line.
        var failures = new List<FluentValidation.Results.ValidationFailure>();
        foreach (var productLines in request.Lines.GroupBy(l => l.ProductId))
        {
            if (!pickList.Lines.Any(l => l.ProductId == productLines.Key))
            {
                failures.Add(new FluentValidation.Results.ValidationFailure(
                    nameof(request.Lines), $"Product {productLines.Key} is not part of pick list {request.PickListId}."));
                continue;
            }

            var pickedQuantity = pickList.Lines.Where(l => l.ProductId == productLines.Key).Sum(l => l.QuantityPicked);
            var requestedQuantity = productLines.Sum(l => l.Quantity);
            var alreadyPackaged = await _repository.GetPackagedQuantityAsync(request.CompanyId, request.PickListId, productLines.Key, cancellationToken);
            if (alreadyPackaged + requestedQuantity > pickedQuantity)
            {
                failures.Add(new FluentValidation.Results.ValidationFailure(
                    nameof(request.Lines),
                    $"Product {productLines.Key}: packaging {requestedQuantity} would exceed the pick list's remaining packageable quantity " +
                    $"({pickedQuantity - alreadyPackaged} of {pickedQuantity} left, {alreadyPackaged} already packaged)."));
            }
        }

        if (failures.Count > 0)
            throw new ValidationException(failures);

        var package = Domain.Packages.Package.Create(
            request.CompanyId,
            request.PickListId,
            request.PackageNumber,
            request.WeightKg,
            request.LengthCm,
            request.WidthCm,
            request.HeightCm,
            request.Lines);

        await _repository.AddAsync(package, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return PackageMapper.MapToDto(package);
    }
}
