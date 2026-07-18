using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Warehouse.Application.Warehouses.Contracts;
using MediatR;

namespace FusionOS.Modules.Warehouse.Application.Warehouses.Commands.UpdateWarehouse;

public sealed class UpdateWarehouseCommandHandler : IRequestHandler<UpdateWarehouseCommand, WarehouseDto>
{
    private readonly IWarehouseRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateWarehouseCommandHandler(IWarehouseRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<WarehouseDto> Handle(UpdateWarehouseCommand request, CancellationToken cancellationToken)
    {
        var warehouse = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (warehouse is null || warehouse.CompanyId != request.CompanyId)
        {
            // No dedicated 404 exception type exists yet in this codebase
            // (ProblemDetailsExceptionHandler only maps ValidationException ->
            // 400 and ForbiddenException -> 403) — matching CreateWarehouseCommandHandler's
            // existing use of ValidationException for a business-rule rejection.
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.Id), "Warehouse not found."),
            });
        }

        warehouse.UpdateDetails(request.BranchId, request.Name, request.Address);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new WarehouseDto(warehouse.Id, warehouse.Name, warehouse.Code, warehouse.Address, warehouse.IsActive, warehouse.CreatedAt);
    }
}
