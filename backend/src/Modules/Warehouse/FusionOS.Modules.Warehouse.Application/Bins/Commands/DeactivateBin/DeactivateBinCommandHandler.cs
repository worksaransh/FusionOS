using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Warehouse.Application.Bins.Contracts;
using MediatR;

namespace FusionOS.Modules.Warehouse.Application.Bins.Commands.DeactivateBin;

public sealed class DeactivateBinCommandHandler : IRequestHandler<DeactivateBinCommand, BinDto>
{
    private readonly IBinRepository _repository;
    private readonly FusionOS.Modules.Warehouse.Application.Warehouses.Contracts.IUnitOfWork _unitOfWork;

    public DeactivateBinCommandHandler(IBinRepository repository, FusionOS.Modules.Warehouse.Application.Warehouses.Contracts.IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BinDto> Handle(DeactivateBinCommand request, CancellationToken cancellationToken)
    {
        var bin = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (bin is null || bin.CompanyId != request.CompanyId)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.Id), "Bin not found."),
            });
        }

        bin.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new BinDto(bin.Id, bin.ZoneId, bin.Name, bin.Code, bin.IsActive, bin.CreatedAt);
    }
}
