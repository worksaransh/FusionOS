using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Inventory.Application.Batches.Contracts;
using FusionOS.Modules.Inventory.Application.Products.Contracts;
using MediatR;

namespace FusionOS.Modules.Inventory.Application.Batches.Commands.CreateBatch;

public sealed class CreateBatchCommandHandler : IRequestHandler<CreateBatchCommand, BatchDto>
{
    private readonly IBatchRepository _repository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateBatchCommandHandler(IBatchRepository repository, IProductRepository productRepository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BatchDto> Handle(CreateBatchCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null || product.CompanyId != request.CompanyId)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.ProductId), "Product not found."),
            });
        }

        if (await _repository.BatchNumberExistsAsync(request.CompanyId, request.ProductId, request.BatchNumber, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.BatchNumber), $"Batch number '{request.BatchNumber}' already exists for this product."),
            });
        }

        var batch = Domain.Batches.Batch.Create(request.CompanyId, request.ProductId, request.BatchNumber, request.QuantityReceived, request.ExpiryDate);

        await _repository.AddAsync(batch, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return BatchMapper.ToDto(batch);
    }
}
