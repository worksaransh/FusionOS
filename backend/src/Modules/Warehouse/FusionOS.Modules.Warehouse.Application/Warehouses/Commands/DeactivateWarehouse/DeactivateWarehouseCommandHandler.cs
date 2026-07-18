using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Warehouse.Application.Warehouses.Contracts;
using MediatR;

namespace FusionOS.Modules.Warehouse.Application.Warehouses.Commands.DeactivateWarehouse;

public sealed class DeactivateWarehouseCommandHandler : IRequestHandler<DeactivateWarehouseCommand, WarehouseDto>
{
    private readonly IWarehouseRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateWarehouseCommandHandler(IWarehouseRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<WarehouseDto> Handle(DeactivateWarehouseCommand request, CancellationToken cancellationToken)
    {
        var warehouse = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (warehouse is null || warehouse.CompanyId != request.CompanyId)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.Id), "Warehouse not found."),
            });
        }

        warehouse.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new WarehouseDto(warehouse.Id, warehouse.Name, warehouse.Code, warehouse.Address, warehouse.IsActive, warehouse.CreatedAt);
    }
}
