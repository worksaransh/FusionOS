using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Crm.Application.Campaigns.Contracts;
using MediatR;

namespace FusionOS.Modules.Crm.Application.Campaigns.Queries.ListCampaigns;

public sealed class ListCampaignsQueryHandler : IRequestHandler<ListCampaignsQuery, PagedResult<CampaignDto>>
{
    private readonly ICampaignRepository _repository;

    public ListCampaignsQueryHandler(ICampaignRepository repository) => _repository = repository;

    public async Task<PagedResult<CampaignDto>> Handle(ListCampaignsQuery request, CancellationToken cancellationToken)
    {
        var campaigns = await _repository.ListAsync(request.CompanyId, request.Search, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, request.Search, cancellationToken);

        var dtos = campaigns.Select(CampaignMapper.ToDto).ToList();

        return new PagedResult<CampaignDto>(dtos, request.Page, request.PageSize, total);
    }
}
