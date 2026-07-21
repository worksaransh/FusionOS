using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Warehouse.Application.Shelves.Contracts;
using MediatR;

namespace FusionOS.Modules.Warehouse.Application.Shelves.Commands.UpdateShelf;

public sealed class UpdateShelfCommandHandler : IRequestHandler<UpdateShelfCommand, ShelfDto>
{
    private readonly IShelfRepository _repository;
    private readonly FusionOS.Modules.Warehouse.Application.Warehouses.Contracts.IUnitOfWork _unitOfWork;

    public UpdateShelfCommandHandler(IShelfRepository repository, FusionOS.Modules.Warehouse.Application.Warehouses.Contracts.IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ShelfDto> Handle(UpdateShelfCommand request, CancellationToken cancellationToken)
    {
        var shelf = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (shelf is null || shelf.CompanyId != request.CompanyId)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.Id), "Shelf not found."),
            });
        }

        shelf.UpdateDetails(request.Name);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ShelfDto(shelf.Id, shelf.RackId, shelf.Name, shelf.Code, shelf.IsActive, shelf.CreatedAt);
    }
}
