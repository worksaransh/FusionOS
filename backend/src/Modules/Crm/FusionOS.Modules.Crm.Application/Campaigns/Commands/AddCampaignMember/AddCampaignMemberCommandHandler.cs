using FusionOS.Modules.Crm.Application.Campaigns.Contracts;
using FusionOS.Modules.Crm.Application.Leads.Contracts;
using MediatR;

namespace FusionOS.Modules.Crm.Application.Campaigns.Commands.AddCampaignMember;

public sealed class AddCampaignMemberCommandHandler : IRequestHandler<AddCampaignMemberCommand, CampaignMembershipDto>
{
    private readonly ICampaignRepository _campaignRepository;
    private readonly ICampaignMembershipRepository _membershipRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddCampaignMemberCommandHandler(
        ICampaignRepository campaignRepository,
        ICampaignMembershipRepository membershipRepository,
        IUnitOfWork unitOfWork)
    {
        _campaignRepository = campaignRepository;
        _membershipRepository = membershipRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CampaignMembershipDto> Handle(AddCampaignMemberCommand request, CancellationToken cancellationToken)
    {
        if (!await _campaignRepository.ExistsAsync(request.CompanyId, request.CampaignId, cancellationToken))
            throw new KeyNotFoundException($"Campaign '{request.CampaignId}' was not found.");

        var existing = await _membershipRepository.FindAsync(request.CompanyId, request.CampaignId, request.MemberEntityType, request.MemberEntityId, cancellationToken);
        if (existing is not null)
            return CampaignMembershipMapper.ToDto(existing);

        var membership = Domain.Campaigns.CampaignMembership.Create(request.CompanyId, request.CampaignId, request.MemberEntityType, request.MemberEntityId);

        await _membershipRepository.AddAsync(membership, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CampaignMembershipMapper.ToDto(membership);
    }
}
