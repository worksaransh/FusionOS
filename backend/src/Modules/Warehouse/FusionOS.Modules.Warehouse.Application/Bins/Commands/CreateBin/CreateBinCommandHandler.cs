using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Warehouse.Application.Bins.Contracts;
using MediatR;

namespace FusionOS.Modules.Warehouse.Application.Bins.Commands.CreateBin;

public sealed class CreateBinCommandHandler : IRequestHandler<CreateBinCommand, BinDto>
{
    private readonly IBinRepository _repository;
    private readonly FusionOS.Modules.Warehouse.Application.Warehouses.Contracts.IUnitOfWork _unitOfWork;

    public CreateBinCommandHandler(IBinRepository repository, FusionOS.Modules.Warehouse.Application.Warehouses.Contracts.IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BinDto> Handle(CreateBinCommand request, CancellationToken cancellationToken)
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
                new FluentValidation.Results.ValidationFailure(nameof(request.Code), $"Bin code '{request.Code}' already exists in this zone."),
            });
        }

        var bin = Domain.Bins.Bin.Create(request.CompanyId, request.ZoneId, request.Name, request.Code);

        await _repository.AddAsync(bin, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new BinDto(bin.Id, bin.ZoneId, bin.Name, bin.Code, bin.IsActive, bin.CreatedAt);
    }
}
