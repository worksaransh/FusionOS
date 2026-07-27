using FusionOS.Modules.Crm.Application.Campaigns.Contracts;
using FusionOS.Modules.Crm.Application.Leads.Contracts;
using MediatR;

namespace FusionOS.Modules.Crm.Application.Campaigns.Commands.RemoveCampaignMember;

public sealed class RemoveCampaignMemberCommandHandler : IRequestHandler<RemoveCampaignMemberCommand, Unit>
{
    private readonly ICampaignMembershipRepository _membershipRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveCampaignMemberCommandHandler(ICampaignMembershipRepository membershipRepository, IUnitOfWork unitOfWork)
    {
        _membershipRepository = membershipRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(RemoveCampaignMemberCommand request, CancellationToken cancellationToken)
    {
        var membership = await _membershipRepository.FindAsync(request.CompanyId, request.CampaignId, request.MemberEntityType, request.MemberEntityId, cancellationToken)
            ?? throw new KeyNotFoundException("This member is not enrolled in the campaign.");

        _membershipRepository.Remove(membership);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
