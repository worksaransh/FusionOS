using FusionOS.Modules.Inventory.Application.Batches.Contracts;
using MediatR;

namespace FusionOS.Modules.Inventory.Application.Batches.Queries.GetBatchById;

public sealed class GetBatchByIdQueryHandler : IRequestHandler<GetBatchByIdQuery, BatchDto?>
{
    private readonly IBatchRepository _repository;

    public GetBatchByIdQueryHandler(IBatchRepository repository) => _repository = repository;

    public async Task<BatchDto?> Handle(GetBatchByIdQuery request, CancellationToken cancellationToken)
    {
        var batch = await _repository.GetByIdAsync(request.CompanyId, request.Id, cancellationToken);
        return batch is null ? null : BatchMapper.ToDto(batch);
    }
}
