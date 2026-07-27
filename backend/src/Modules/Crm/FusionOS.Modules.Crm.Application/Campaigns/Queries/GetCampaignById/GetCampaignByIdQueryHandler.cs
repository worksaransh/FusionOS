using FusionOS.Modules.Crm.Application.Campaigns.Contracts;
using MediatR;

namespace FusionOS.Modules.Crm.Application.Campaigns.Queries.GetCampaignById;

public sealed class GetCampaignByIdQueryHandler : IRequestHandler<GetCampaignByIdQuery, CampaignWithMembersDto>
{
    private readonly ICampaignRepository _campaignRepository;
    private readonly ICampaignMembershipRepository _membershipRepository;

    public GetCampaignByIdQueryHandler(ICampaignRepository campaignRepository, ICampaignMembershipRepository membershipRepository)
    {
        _campaignRepository = campaignRepository;
        _membershipRepository = membershipRepository;
    }

    public async Task<CampaignWithMembersDto> Handle(GetCampaignByIdQuery request, CancellationToken cancellationToken)
    {
        var campaign = await _campaignRepository.GetByIdAsync(request.CompanyId, request.CampaignId, cancellationToken)
            ?? throw new KeyNotFoundException($"Campaign '{request.CampaignId}' was not found.");

        var members = await _membershipRepository.ListByCampaignAsync(request.CompanyId, request.CampaignId, cancellationToken);

        return new CampaignWithMembersDto(
            CampaignMapper.ToDto(campaign),
            members.Select(CampaignMembershipMapper.ToDto).ToList());
    }
}
