using FusionOS.Modules.Crm.Application.Leads.Contracts;
using MediatR;

namespace FusionOS.Modules.Crm.Application.Leads.Commands.CreateLead;

public sealed class CreateLeadCommandHandler : IRequestHandler<CreateLeadCommand, LeadDto>
{
    private readonly ILeadRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateLeadCommandHandler(ILeadRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<LeadDto> Handle(CreateLeadCommand request, CancellationToken cancellationToken)
    {
        var lead = Domain.Leads.Lead.Create(request.CompanyId, request.Name, request.ContactEmail, request.ContactPhone, request.Source);

        await _repository.AddAsync(lead, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return LeadMapper.ToDto(lead);
    }
}
