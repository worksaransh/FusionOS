using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.CostCenters.Commands.CreateCostCenter;
using FusionOS.Modules.Finance.Application.CostCenters.Contracts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.CostCenters.Commands.UpdateCostCenter;

public sealed class UpdateCostCenterCommandHandler : IRequestHandler<UpdateCostCenterCommand, CostCenterDto>
{
    private readonly ICostCenterRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCostCenterCommandHandler(ICostCenterRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CostCenterDto> Handle(UpdateCostCenterCommand request, CancellationToken cancellationToken)
    {
        var costCenter = await _repository.GetByIdAsync(request.CompanyId, request.CostCenterId, cancellationToken)
            ?? throw new KeyNotFoundException($"Cost center '{request.CostCenterId}' was not found.");

        costCenter.UpdateDetails(request.Name);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateCostCenterCommandHandler.MapToDto(costCenter);
    }
}
