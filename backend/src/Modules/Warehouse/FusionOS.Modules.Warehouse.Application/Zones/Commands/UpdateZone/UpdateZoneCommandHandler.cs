using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Warehouse.Application.Zones.Contracts;
using MediatR;

namespace FusionOS.Modules.Warehouse.Application.Zones.Commands.UpdateZone;

public sealed class UpdateZoneCommandHandler : IRequestHandler<UpdateZoneCommand, ZoneDto>
{
    private readonly IZoneRepository _repository;
    private readonly FusionOS.Modules.Warehouse.Application.Warehouses.Contracts.IUnitOfWork _unitOfWork;

    public UpdateZoneCommandHandler(IZoneRepository repository, FusionOS.Modules.Warehouse.Application.Warehouses.Contracts.IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ZoneDto> Handle(UpdateZoneCommand request, CancellationToken cancellationToken)
    {
        var zone = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (zone is null || zone.CompanyId != request.CompanyId)
        {
            // No dedicated 404 exception type exists yet in this codebase
            // (ProblemDetailsExceptionHandler only maps ValidationException ->
            // 400 and ForbiddenException -> 403) — matching CreateZoneCommandHandler's
            // existing use of ValidationException for a business-rule rejection.
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.Id), "Zone not found."),
            });
        }

        zone.UpdateDetails(request.Name);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ZoneDto(zone.Id, zone.WarehouseId, zone.Name, zone.Code, zone.IsActive, zone.CreatedAt);
    }
}
