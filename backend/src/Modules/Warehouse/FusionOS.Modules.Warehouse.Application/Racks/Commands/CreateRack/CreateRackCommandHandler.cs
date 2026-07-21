using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Warehouse.Application.Racks.Contracts;
using MediatR;

namespace FusionOS.Modules.Warehouse.Application.Racks.Commands.CreateRack;

public sealed class CreateRackCommandHandler : IRequestHandler<CreateRackCommand, RackDto>
{
    private readonly IRackRepository _repository;
    private readonly FusionOS.Modules.Warehouse.Application.Warehouses.Contracts.IUnitOfWork _unitOfWork;

    public CreateRackCommandHandler(IRackRepository repository, FusionOS.Modules.Warehouse.Application.Warehouses.Contracts.IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<RackDto> Handle(CreateRackCommand request, CancellationToken cancellationToken)
    {
        if (!await _repository.ZoneExistsAsync(request.CompanyId, request.ZoneId, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.ZoneId), "Zone does not exist for this company."),
            });
        }

        if (await _repository.CodeExistsAsync(request.CompanyId, request.ZoneId, request.Code, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.Code), $"Rack code '{request.Code}' already exists in this zone."),
            });
        }

        var rack = Domain.Racks.Rack.Create(request.CompanyId, request.ZoneId, request.Name, request.Code);

        await _repository.AddAsync(rack, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new RackDto(rack.Id, rack.ZoneId, rack.Name, rack.Code, rack.IsActive, rack.CreatedAt);
    }
}
