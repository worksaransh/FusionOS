using FusionOS.Modules.Warehouse.Application.Bins.Contracts;
using MediatR;

namespace FusionOS.Modules.Warehouse.Application.Bins.Queries.GetBinById;

public sealed class GetBinByIdQueryHandler : IRequestHandler<GetBinByIdQuery, BinDto?>
{
    private readonly IBinRepository _repository;

    public GetBinByIdQueryHandler(IBinRepository repository) => _repository = repository;

    public async Task<BinDto?> Handle(GetBinByIdQuery request, CancellationToken cancellationToken)
    {
        var bin = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (bin is null || bin.CompanyId != request.CompanyId)
            return null;

        return new BinDto(bin.Id, bin.ZoneId, bin.Name, bin.Code, bin.IsActive, bin.CreatedAt, bin.ShelfId);
    }
}
