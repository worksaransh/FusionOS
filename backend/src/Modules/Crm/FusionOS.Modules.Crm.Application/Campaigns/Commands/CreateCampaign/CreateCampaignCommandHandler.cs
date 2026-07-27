using FusionOS.Modules.Crm.Application.Campaigns.Contracts;
using FusionOS.Modules.Crm.Application.Leads.Contracts;
using MediatR;

namespace FusionOS.Modules.Crm.Application.Campaigns.Commands.CreateCampaign;

public sealed class CreateCampaignCommandHandler : IRequestHandler<CreateCampaignCommand, CampaignDto>
{
    private readonly ICampaignRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCampaignCommandHandler(ICampaignRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CampaignDto> Handle(CreateCampaignCommand request, CancellationToken cancellationToken)
    {
        var campaign = Domain.Campaigns.Campaign.Create(request.CompanyId, request.Name, request.Description, request.StartDate, request.EndDate);

        await _repository.AddAsync(campaign, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CampaignMapper.ToDto(campaign);
    }
}
