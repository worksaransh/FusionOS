using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Warehouse.Application.Bins.Contracts;
using FusionOS.Modules.Warehouse.Application.PickLists.Contracts;
using FusionOS.Modules.Warehouse.Application.Zones.Contracts;
using MediatR;

namespace FusionOS.Modules.Warehouse.Application.PickLists.Commands.CreatePickList;

public sealed class CreatePickListCommandHandler : IRequestHandler<CreatePickListCommand, PickListDto>
{
    private readonly IPickListRepository _repository;
    private readonly IZoneRepository _zoneRepository;
    private readonly IBinRepository _binRepository;
    private readonly FusionOS.Modules.Warehouse.Application.Warehouses.Contracts.IUnitOfWork _unitOfWork;

    public CreatePickListCommandHandler(
        IPickListRepository repository,
        IZoneRepository zoneRepository,
        IBinRepository binRepository,
        FusionOS.Modules.Warehouse.Application.Warehouses.Contracts.IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _zoneRepository = zoneRepository;
        _binRepository = binRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PickListDto> Handle(CreatePickListCommand request, CancellationToken cancellationToken)
    {
        // IZoneRepository.WarehouseExistsAsync is reused as-is here — it already does exactly what
        // this handler needs (confirm the warehouse exists for this company), same-module reuse,
        // no new API surface needed.
        if (!await _zoneRepository.WarehouseExistsAsync(request.CompanyId, request.WarehouseId, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.WarehouseId), "Warehouse does not exist for this company."),
            });
        }

        // Lines are grouped by BinId before checking, so the same bin referenced by
        // several request lines is only looked up once instead of once per line.
        var failures = new List<FluentValidation.Results.ValidationFailure>();
        foreach (var binId in request.Lines.Where(l => l.BinId is not null).Select(l => l.BinId!.Value).Distinct())
        {
            if (!await _binRepository.ExistsAsync(request.CompanyId, binId, cancellationToken))
            {
                failures.Add(new FluentValidation.Results.ValidationFailure(nameof(request.Lines), $"Bin '{binId}' does not exist for this company."));
            }
        }
        if (failures.Count > 0)
        {
            throw new ValidationException(failures);
        }

        var pickList = Domain.PickLists.PickList.Create(request.CompanyId, request.WarehouseId, request.SalesOrderId, request.Lines);

        await _repository.AddAsync(pickList, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return PickListMapper.MapToDto(pickList);
    }
}
