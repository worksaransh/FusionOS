using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Warehouse.Application.Packages.Contracts;
using MediatR;

namespace FusionOS.Modules.Warehouse.Application.Packages.Queries.ListPackagesByPickList;

public sealed class ListPackagesByPickListQueryHandler : IRequestHandler<ListPackagesByPickListQuery, PagedResult<PackageDto>>
{
    private readonly IPackageRepository _repository;

    public ListPackagesByPickListQueryHandler(IPackageRepository repository) => _repository = repository;

    public async Task<PagedResult<PackageDto>> Handle(ListPackagesByPickListQuery request, CancellationToken cancellationToken)
    {
        var packages = await _repository.ListByPickListAsync(request.CompanyId, request.PickListId, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountByPickListAsync(request.CompanyId, request.PickListId, cancellationToken);

        var dtos = packages.Select(PackageMapper.MapToDto).ToList();

        return new PagedResult<PackageDto>(dtos, request.Page, request.PageSize, total);
    }
}
