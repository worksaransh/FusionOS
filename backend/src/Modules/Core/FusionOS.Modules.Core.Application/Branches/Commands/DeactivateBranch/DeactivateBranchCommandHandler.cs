using FusionOS.Modules.Core.Application.Branches.Commands.CreateBranch;
using FusionOS.Modules.Core.Application.Branches.Contracts;
using FusionOS.Modules.Core.Application.Companies.Contracts;
using MediatR;

namespace FusionOS.Modules.Core.Application.Branches.Commands.DeactivateBranch;

public sealed class DeactivateBranchCommandHandler : IRequestHandler<DeactivateBranchCommand, BranchDto>
{
    private readonly IBranchRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateBranchCommandHandler(IBranchRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BranchDto> Handle(DeactivateBranchCommand request, CancellationToken cancellationToken)
    {
        var branch = await _repository.GetByIdAsync(request.CompanyId, request.BranchId, cancellationToken)
            ?? throw new KeyNotFoundException($"Branch '{request.BranchId}' was not found.");

        branch.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateBranchCommandHandler.MapToDto(branch);
    }
}
