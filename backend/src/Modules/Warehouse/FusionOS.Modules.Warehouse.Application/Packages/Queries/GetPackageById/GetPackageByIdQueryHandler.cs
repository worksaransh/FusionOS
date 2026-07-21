using FusionOS.Modules.Warehouse.Application.Packages.Contracts;
using MediatR;

namespace FusionOS.Modules.Warehouse.Application.Packages.Queries.GetPackageById;

public sealed class GetPackageByIdQueryHandler : IRequestHandler<GetPackageByIdQuery, PackageDto?>
{
    private readonly IPackageRepository _repository;

    public GetPackageByIdQueryHandler(IPackageRepository repository) => _repository = repository;

    public async Task<PackageDto?> Handle(GetPackageByIdQuery request, CancellationToken cancellationToken)
    {
        var package = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (package is null || package.CompanyId != request.CompanyId)
            return null;

        return PackageMapper.MapToDto(package);
    }
}
