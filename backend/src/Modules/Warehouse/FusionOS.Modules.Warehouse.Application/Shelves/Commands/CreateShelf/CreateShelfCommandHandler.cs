using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Warehouse.Application.Shelves.Contracts;
using MediatR;

namespace FusionOS.Modules.Warehouse.Application.Shelves.Commands.CreateShelf;

public sealed class CreateShelfCommandHandler : IRequestHandler<CreateShelfCommand, ShelfDto>
{
    private readonly IShelfRepository _repository;
    private readonly FusionOS.Modules.Warehouse.Application.Warehouses.Contracts.IUnitOfWork _unitOfWork;

    public CreateShelfCommandHandler(IShelfRepository repository, FusionOS.Modules.Warehouse.Application.Warehouses.Contracts.IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ShelfDto> Handle(CreateShelfCommand request, CancellationToken cancellationToken)
    {
        if (!await _repository.RackExistsAsync(request.CompanyId, request.RackId, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.RackId), "Rack does not exist for this company."),
            });
        }

        if (await _repository.CodeExistsAsync(request.CompanyId, request.RackId, request.Code, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.Code), $"Shelf code '{request.Code}' already exists in this rack."),
            });
        }

        var shelf = Domain.Shelves.Shelf.Create(request.CompanyId, request.RackId, request.Name, request.Code);

        await _repository.AddAsync(shelf, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ShelfDto(shelf.Id, shelf.RackId, shelf.Name, shelf.Code, shelf.IsActive, shelf.CreatedAt);
    }
}
