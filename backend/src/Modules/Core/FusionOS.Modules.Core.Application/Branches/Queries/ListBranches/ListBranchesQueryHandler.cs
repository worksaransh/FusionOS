using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Core.Application.Branches.Commands.CreateBranch;
using FusionOS.Modules.Core.Application.Branches.Contracts;
using MediatR;

namespace FusionOS.Modules.Core.Application.Branches.Queries.ListBranches;

public sealed class ListBranchesQueryHandler : IRequestHandler<ListBranchesQuery, PagedResult<BranchDto>>
{
    private readonly IBranchRepository _repository;

    public ListBranchesQueryHandler(IBranchRepository repository) => _repository = repository;

    public async Task<PagedResult<BranchDto>> Handle(ListBranchesQuery request, CancellationToken cancellationToken)
    {
        var branches = await _repository.ListAsync(request.CompanyId, request.Search, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, request.Search, cancellationToken);

        var dtos = branches.Select(CreateBranchCommandHandler.MapToDto).ToList();

        return new PagedResult<BranchDto>(dtos, request.Page, request.PageSize, total);
    }
}
