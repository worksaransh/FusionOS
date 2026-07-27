using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Contracts;
using MediatR;

namespace FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Commands.CreateBillOfMaterials;

public sealed class CreateBillOfMaterialsCommandHandler : IRequestHandler<CreateBillOfMaterialsCommand, BillOfMaterialsDto>
{
    private readonly IBillOfMaterialsRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateBillOfMaterialsCommandHandler(IBillOfMaterialsRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BillOfMaterialsDto> Handle(CreateBillOfMaterialsCommand request, CancellationToken cancellationToken)
    {
        var code = request.Code?.Trim().ToUpperInvariant() ?? string.Empty;
        if (await _repository.CodeExistsAsync(request.CompanyId, code, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.Code), $"A bill of materials with code '{code}' already exists for this company."),
            });
        }

        // A product's first-ever BOM becomes its primary automatically; a later BOM for a
        // product that already has an active one is created as a non-primary alternative,
        // so IsPrimary never ends up true on two rows for the same product (Alternative
        // BOM, Phase 6, 2026-07-22 — fixes a gap where every new BOM defaulted to primary
        // regardless of what already existed).
        var existingActiveBoms = await _repository.ListActiveByProductIdAsync(request.CompanyId, request.ProductId, cancellationToken);
        var isPrimary = (existingActiveBoms?.Count ?? 0) == 0;

        var bom = Domain.BillOfMaterials.BillOfMaterials.Create(request.CompanyId, request.Code, request.Name, request.ProductId, request.Lines, isPrimary);

        await _repository.AddAsync(bom, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return BillOfMaterialsMapper.ToDto(bom);
    }
}
