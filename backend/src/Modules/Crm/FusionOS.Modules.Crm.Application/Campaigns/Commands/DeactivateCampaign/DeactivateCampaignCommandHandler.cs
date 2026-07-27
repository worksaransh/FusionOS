using FusionOS.Modules.Crm.Application.Campaigns.Contracts;
using FusionOS.Modules.Crm.Application.Leads.Contracts;
using MediatR;

namespace FusionOS.Modules.Crm.Application.Campaigns.Commands.DeactivateCampaign;

public sealed class DeactivateCampaignCommandHandler : IRequestHandler<DeactivateCampaignCommand, CampaignDto>
{
    private readonly ICampaignRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateCampaignCommandHandler(ICampaignRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CampaignDto> Handle(DeactivateCampaignCommand request, CancellationToken cancellationToken)
    {
        var campaign = await _repository.GetByIdAsync(request.CompanyId, request.CampaignId, cancellationToken)
            ?? throw new KeyNotFoundException($"Campaign '{request.CampaignId}' was not found.");

        campaign.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CampaignMapper.ToDto(campaign);
    }
}
