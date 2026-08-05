using FusionOS.Modules.Crm.Application.Campaigns.Contracts;
using FusionOS.Modules.Crm.Application.Leads.Contracts;
using MediatR;

namespace FusionOS.Modules.Crm.Application.Campaigns.Commands.UpdateCampaign;

public sealed class UpdateCampaignCommandHandler : IRequestHandler<UpdateCampaignCommand, CampaignDto>
{
    private readonly ICampaignRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCampaignCommandHandler(ICampaignRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CampaignDto> Handle(UpdateCampaignCommand request, CancellationToken cancellationToken)
    {
        var campaign = await _repository.GetByIdAsync(request.CompanyId, request.CampaignId, cancellationToken)
            ?? throw new KeyNotFoundException($"Campaign '{request.CampaignId}' was not found.");

        campaign.UpdateDetails(request.Name, request.Description, request.StartDate, request.EndDate);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CampaignMapper.ToDto(campaign);
    }
}
