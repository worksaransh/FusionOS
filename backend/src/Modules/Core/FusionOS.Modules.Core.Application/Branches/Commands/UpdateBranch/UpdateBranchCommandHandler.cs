using FusionOS.Modules.Core.Application.Branches.Commands.CreateBranch;
using FusionOS.Modules.Core.Application.Branches.Contracts;
using FusionOS.Modules.Core.Application.Companies.Contracts;
using MediatR;

namespace FusionOS.Modules.Core.Application.Branches.Commands.UpdateBranch;

public sealed class UpdateBranchCommandHandler : IRequestHandler<UpdateBranchCommand, BranchDto>
{
    private readonly IBranchRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateBranchCommandHandler(IBranchRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BranchDto> Handle(UpdateBranchCommand request, CancellationToken cancellationToken)
    {
        var branch = await _repository.GetByIdAsync(request.CompanyId, request.BranchId, cancellationToken)
            ?? throw new KeyNotFoundException($"Branch '{request.BranchId}' was not found.");

        branch.UpdateDetails(request.Name, request.IsHeadOffice);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateBranchCommandHandler.MapToDto(branch);
    }
}
