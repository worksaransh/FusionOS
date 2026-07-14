using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Warehouse.Application.Zones.Contracts;
using MediatR;

namespace FusionOS.Modules.Warehouse.Application.Zones.Commands.CreateZone;

public sealed class CreateZoneCommandHandler : IRequestHandler<CreateZoneCommand, ZoneDto>
{
    private readonly IZoneRepository _repository;
    private readonly FusionOS.Modules.Warehouse.Application.Warehouses.Contracts.IUnitOfWork _unitOfWork;

    public CreateZoneCommandHandler(IZoneRepository repository, FusionOS.Modules.Warehouse.Application.Warehouses.Contracts.IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ZoneDto> Handle(CreateZoneCommand request, CancellationToken cancellationToken)
    {
        if (!await _repository.WarehouseExistsAsync(request.CompanyId, request.WarehouseId, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.WarehouseId), "Warehouse does not exist for this company."),
            });
        }

        if (await _repository.CodeExistsAsync(request.CompanyId, request.WarehouseId, request.Code, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.Code), $"Zone code '{request.Code}' already exists in this warehouse."),
            });
        }

        var zone = Domain.Zones.Zone.Create(request.CompanyId, request.WarehouseId, request.Name, request.Code);

        await _repository.AddAsync(zone, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ZoneDto(zone.Id, zone.WarehouseId, zone.Name, zone.Code, zone.IsActive, zone.CreatedAt);
    }
}
