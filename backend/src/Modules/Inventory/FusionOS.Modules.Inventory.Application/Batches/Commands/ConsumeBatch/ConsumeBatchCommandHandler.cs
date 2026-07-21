using FusionOS.Modules.Inventory.Application.Batches.Contracts;
using FusionOS.Modules.Inventory.Application.Products.Contracts;
using MediatR;

namespace FusionOS.Modules.Inventory.Application.Batches.Commands.ConsumeBatch;

public sealed class ConsumeBatchCommandHandler : IRequestHandler<ConsumeBatchCommand, BatchDto>
{
    private readonly IBatchRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public ConsumeBatchCommandHandler(IBatchRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BatchDto> Handle(ConsumeBatchCommand request, CancellationToken cancellationToken)
    {
        var batch = await _repository.GetByIdAsync(request.CompanyId, request.BatchId, cancellationToken)
            ?? throw new KeyNotFoundException($"Batch '{request.BatchId}' was not found.");

        batch.Consume(request.Quantity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return BatchMapper.ToDto(batch);
    }
}
