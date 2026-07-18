using FusionOS.Modules.Crm.Application.Leads.Contracts;
using MediatR;

namespace FusionOS.Modules.Crm.Application.Leads.Commands.DisqualifyLead;

public sealed class DisqualifyLeadCommandHandler : IRequestHandler<DisqualifyLeadCommand, LeadDto>
{
    private readonly ILeadRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DisqualifyLeadCommandHandler(ILeadRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<LeadDto> Handle(DisqualifyLeadCommand request, CancellationToken cancellationToken)
    {
        var lead = await _repository.GetByIdAsync(request.CompanyId, request.LeadId, cancellationToken)
            ?? throw new KeyNotFoundException($"Lead '{request.LeadId}' was not found.");

        lead.Disqualify();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return LeadMapper.ToDto(lead);
    }
}
