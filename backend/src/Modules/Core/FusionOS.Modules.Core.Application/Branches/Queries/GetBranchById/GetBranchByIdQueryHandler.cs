using FusionOS.Modules.Core.Application.Branches.Commands.CreateBranch;
using FusionOS.Modules.Core.Application.Branches.Contracts;
using MediatR;

namespace FusionOS.Modules.Core.Application.Branches.Queries.GetBranchById;

public sealed class GetBranchByIdQueryHandler : IRequestHandler<GetBranchByIdQuery, BranchDto>
{
    private readonly IBranchRepository _repository;

    public GetBranchByIdQueryHandler(IBranchRepository repository) => _repository = repository;

    public async Task<BranchDto> Handle(GetBranchByIdQuery request, CancellationToken cancellationToken)
    {
        var branch = await _repository.GetByIdAsync(request.CompanyId, request.BranchId, cancellationToken)
            ?? throw new KeyNotFoundException($"Branch '{request.BranchId}' was not found.");

        return CreateBranchCommandHandler.MapToDto(branch);
    }
}
