using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Warehouse.Application.Racks.Contracts;
using MediatR;

namespace FusionOS.Modules.Warehouse.Application.Racks.Commands.DeactivateRack;

public sealed class DeactivateRackCommandHandler : IRequestHandler<DeactivateRackCommand, RackDto>
{
    private readonly IRackRepository _repository;
    private readonly FusionOS.Modules.Warehouse.Application.Warehouses.Contracts.IUnitOfWork _unitOfWork;

    public DeactivateRackCommandHandler(IRackRepository repository, FusionOS.Modules.Warehouse.Application.Warehouses.Contracts.IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<RackDto> Handle(DeactivateRackCommand request, CancellationToken cancellationToken)
    {
        var rack = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (rack is null || rack.CompanyId != request.CompanyId)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.Id), "Rack not found."),
            });
        }

        rack.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new RackDto(rack.Id, rack.ZoneId, rack.Name, rack.Code, rack.IsActive, rack.CreatedAt);
    }
}
