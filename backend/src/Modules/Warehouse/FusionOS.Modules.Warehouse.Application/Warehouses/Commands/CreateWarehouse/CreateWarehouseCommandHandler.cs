using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Warehouse.Application.Warehouses.Contracts;
using MediatR;

namespace FusionOS.Modules.Warehouse.Application.Warehouses.Commands.CreateWarehouse;

public sealed class CreateWarehouseCommandHandler : IRequestHandler<CreateWarehouseCommand, WarehouseDto>
{
    private readonly IWarehouseRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateWarehouseCommandHandler(IWarehouseRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<WarehouseDto> Handle(CreateWarehouseCommand request, CancellationToken cancellationToken)
    {
        if (await _repository.CodeExistsAsync(request.CompanyId, request.Code, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.Code), $"Warehouse code '{request.Code}' already exists for this company."),
            });
        }

        var warehouse = Domain.Warehouses.Warehouse.Create(request.CompanyId, request.BranchId, request.Name, request.Code, request.Address);

        await _repository.AddAsync(warehouse, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new WarehouseDto(warehouse.Id, warehouse.Name, warehouse.Code, warehouse.Address, warehouse.IsActive, warehouse.CreatedAt);
    }
}
