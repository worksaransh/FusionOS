using FusionOS.Modules.Marketplace.Application.PluginInstallations.Contracts;
using MediatR;

namespace FusionOS.Modules.Marketplace.Application.PluginInstallations.Queries.ListPluginInstallations;

public sealed class ListPluginInstallationsQueryHandler : IRequestHandler<ListPluginInstallationsQuery, PagedResult<PluginInstallationDto>>
{
    private readonly IPluginInstallationRepository _repository;

    public ListPluginInstallationsQueryHandler(IPluginInstallationRepository repository) => _repository = repository;

    public async Task<PagedResult<PluginInstallationDto>> Handle(ListPluginInstallationsQuery request, CancellationToken cancellationToken)
    {
        var installations = await _repository.ListAsync(request.CompanyId, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, cancellationToken);

        var dtos = installations.Select(PluginInstallationMapper.ToDto).ToList();

        return new PagedResult<PluginInstallationDto>(dtos, request.Page, request.PageSize, total);
    }
}
