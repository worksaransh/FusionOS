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

        var bom = Domain.BillOfMaterials.BillOfMaterials.Create(request.CompanyId, request.Code, request.Name, request.ProductId, request.Lines);

        await _repository.AddAsync(bom, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return BillOfMaterialsMapper.ToDto(bom);
    }
}
