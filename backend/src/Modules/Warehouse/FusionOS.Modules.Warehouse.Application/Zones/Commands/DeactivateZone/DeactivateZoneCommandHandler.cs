using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Warehouse.Application.Zones.Contracts;
using MediatR;

namespace FusionOS.Modules.Warehouse.Application.Zones.Commands.DeactivateZone;

public sealed class DeactivateZoneCommandHandler : IRequestHandler<DeactivateZoneCommand, ZoneDto>
{
    private readonly IZoneRepository _repository;
    private readonly FusionOS.Modules.Warehouse.Application.Warehouses.Contracts.IUnitOfWork _unitOfWork;

    public DeactivateZoneCommandHandler(IZoneRepository repository, FusionOS.Modules.Warehouse.Application.Warehouses.Contracts.IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ZoneDto> Handle(DeactivateZoneCommand request, CancellationToken cancellationToken)
    {
        var zone = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (zone is null || zone.CompanyId != request.CompanyId)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.Id), "Zone not found."),
            });
        }

        zone.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ZoneDto(zone.Id, zone.WarehouseId, zone.Name, zone.Code, zone.IsActive, zone.CreatedAt);
    }
}
