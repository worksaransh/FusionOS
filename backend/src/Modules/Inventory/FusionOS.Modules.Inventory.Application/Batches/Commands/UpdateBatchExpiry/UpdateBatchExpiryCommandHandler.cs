using FusionOS.Modules.Inventory.Application.Batches.Contracts;
using FusionOS.Modules.Inventory.Application.Products.Contracts;
using MediatR;

namespace FusionOS.Modules.Inventory.Application.Batches.Commands.UpdateBatchExpiry;

public sealed class UpdateBatchExpiryCommandHandler : IRequestHandler<UpdateBatchExpiryCommand, BatchDto>
{
    private readonly IBatchRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateBatchExpiryCommandHandler(IBatchRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BatchDto> Handle(UpdateBatchExpiryCommand request, CancellationToken cancellationToken)
    {
        var batch = await _repository.GetByIdAsync(request.CompanyId, request.BatchId, cancellationToken)
            ?? throw new KeyNotFoundException($"Batch '{request.BatchId}' was not found.");

        batch.AdjustExpiry(request.NewExpiry);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return BatchMapper.ToDto(batch);
    }
}
