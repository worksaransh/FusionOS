using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Warehouse.Application.Bins.Contracts;
using FusionOS.Modules.Warehouse.Application.Racks.Contracts;
using FusionOS.Modules.Warehouse.Application.Shelves.Contracts;
using MediatR;

namespace FusionOS.Modules.Warehouse.Application.Bins.Commands.AssignBinShelf;

/// <summary>
/// Validates that the Shelf exists and — for data-integrity — that the
/// Shelf's Rack's Zone matches this bin's own (required) ZoneId, rejecting
/// the assignment otherwise: a bin's precise shelf location must always be
/// consistent with the zone it's actually required to belong to. Needs
/// IShelfRepository and IRackRepository (in addition to IBinRepository) to
/// walk Shelf -> Rack -> Zone, since neither repository alone can answer
/// "which zone does this shelf ultimately belong to".
/// </summary>
public sealed class AssignBinShelfCommandHandler : IRequestHandler<AssignBinShelfCommand, BinDto>
{
    private readonly IBinRepository _binRepository;
    private readonly IShelfRepository _shelfRepository;
    private readonly IRackRepository _rackRepository;
    private readonly FusionOS.Modules.Warehouse.Application.Warehouses.Contracts.IUnitOfWork _unitOfWork;

    public AssignBinShelfCommandHandler(
        IBinRepository binRepository,
        IShelfRepository shelfRepository,
        IRackRepository rackRepository,
        FusionOS.Modules.Warehouse.Application.Warehouses.Contracts.IUnitOfWork unitOfWork)
    {
        _binRepository = binRepository;
        _shelfRepository = shelfRepository;
        _rackRepository = rackRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BinDto> Handle(AssignBinShelfCommand request, CancellationToken cancellationToken)
    {
        var bin = await _binRepository.GetByIdAsync(request.BinId, cancellationToken);
        if (bin is null || bin.CompanyId != request.CompanyId)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.BinId), "Bin not found."),
            });
        }

        if (request.ShelfId is null)
        {
            bin.AssignShelf(null);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return new BinDto(bin.Id, bin.ZoneId, bin.Name, bin.Code, bin.IsActive, bin.CreatedAt, bin.ShelfId);
        }

        var shelf = await _shelfRepository.GetByIdAsync(request.ShelfId.Value, cancellationToken);
        if (shelf is null || shelf.CompanyId != request.CompanyId)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.ShelfId), "Shelf does not exist for this company."),
            });
        }

        var rack = await _rackRepository.GetByIdAsync(shelf.RackId, cancellationToken);
        if (rack is null || rack.CompanyId != request.CompanyId)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.ShelfId), "Shelf's rack does not exist for this company."),
            });
        }

        if (rack.ZoneId != bin.ZoneId)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.ShelfId), "Shelf's rack belongs to a different zone than this bin."),
            });
        }

        bin.AssignShelf(shelf.Id);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new BinDto(bin.Id, bin.ZoneId, bin.Name, bin.Code, bin.IsActive, bin.CreatedAt, bin.ShelfId);
    }
}
