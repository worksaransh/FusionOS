using FusionOS.Modules.Crm.Application.Leads.Contracts;
using MediatR;

namespace FusionOS.Modules.Crm.Application.Leads.Commands.QualifyLead;

public sealed class QualifyLeadCommandHandler : IRequestHandler<QualifyLeadCommand, LeadDto>
{
    private readonly ILeadRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public QualifyLeadCommandHandler(ILeadRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<LeadDto> Handle(QualifyLeadCommand request, CancellationToken cancellationToken)
    {
        var lead = await _repository.GetByIdAsync(request.CompanyId, request.LeadId, cancellationToken)
            ?? throw new KeyNotFoundException($"Lead '{request.LeadId}' was not found.");

        lead.Qualify();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return LeadMapper.ToDto(lead);
    }
}
