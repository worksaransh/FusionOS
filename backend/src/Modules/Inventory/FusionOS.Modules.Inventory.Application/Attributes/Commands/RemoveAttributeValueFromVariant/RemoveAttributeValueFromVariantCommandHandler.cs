using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Inventory.Application.Attributes.Contracts;
using FusionOS.Modules.Inventory.Application.Products.Contracts;
using MediatR;

namespace FusionOS.Modules.Inventory.Application.Attributes.Commands.RemoveAttributeValueFromVariant;

public sealed class RemoveAttributeValueFromVariantCommandHandler : IRequestHandler<RemoveAttributeValueFromVariantCommand, Unit>
{
    private readonly IProductVariantAttributeValueRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveAttributeValueFromVariantCommandHandler(IProductVariantAttributeValueRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(RemoveAttributeValueFromVariantCommand request, CancellationToken cancellationToken)
    {
        var assignment = await _repository.GetByIdAsync(request.CompanyId, request.AssignmentId, cancellationToken);
        if (assignment is null || assignment.ProductId != request.ProductId || assignment.VariantId != request.VariantId)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.AssignmentId), "Attribute assignment not found."),
            });
        }

        _repository.Remove(assignment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
