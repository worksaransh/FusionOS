using FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Contracts;
using MediatR;

namespace FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Commands.CreateNewBomVersion;

public sealed class CreateNewBomVersionCommandHandler : IRequestHandler<CreateNewBomVersionCommand, BillOfMaterialsDto>
{
    private readonly IBillOfMaterialsRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateNewBomVersionCommandHandler(IBillOfMaterialsRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BillOfMaterialsDto> Handle(CreateNewBomVersionCommand request, CancellationToken cancellationToken)
    {
        var bom = await _repository.GetByIdAsync(request.CompanyId, request.BillOfMaterialsId, cancellationToken)
            ?? throw new KeyNotFoundException($"Bill of materials '{request.BillOfMaterialsId}' was not found.");

        var newVersion = bom.CreateNewVersion(request.Name, request.Lines);

        // bom is already tracked by the DbContext (loaded via GetByIdAsync), so its
        // IsCurrentVersion = false mutation is picked up by EF change tracking with no
        // explicit "update" call — same reliance DeactivateBillOfMaterialsCommandHandler
        // has on SaveChangesAsync alone. newVersion is a brand-new row and must be added.
        await _repository.AddAsync(newVersion, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return BillOfMaterialsMapper.ToDto(newVersion);
    }
}
