using FusionOS.Modules.Crm.Application.Leads.Contracts;
using MediatR;

namespace FusionOS.Modules.Crm.Application.Leads.Commands.AssignLeadOwner;

public sealed class AssignLeadOwnerCommandHandler : IRequestHandler<AssignLeadOwnerCommand, LeadDto>
{
    private readonly ILeadRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public AssignLeadOwnerCommandHandler(ILeadRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<LeadDto> Handle(AssignLeadOwnerCommand request, CancellationToken cancellationToken)
    {
        var lead = await _repository.GetByIdAsync(request.CompanyId, request.LeadId, cancellationToken)
            ?? throw new KeyNotFoundException($"Lead '{request.LeadId}' was not found.");

        lead.AssignOwner(request.OwnerId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return LeadMapper.ToDto(lead);
    }
}
