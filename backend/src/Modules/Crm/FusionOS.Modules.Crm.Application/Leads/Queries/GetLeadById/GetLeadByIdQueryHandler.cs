using FusionOS.Modules.Crm.Application.Leads.Contracts;
using MediatR;

namespace FusionOS.Modules.Crm.Application.Leads.Queries.GetLeadById;

public sealed class GetLeadByIdQueryHandler : IRequestHandler<GetLeadByIdQuery, LeadDto>
{
    private readonly ILeadRepository _repository;

    public GetLeadByIdQueryHandler(ILeadRepository repository) => _repository = repository;

    public async Task<LeadDto> Handle(GetLeadByIdQuery request, CancellationToken cancellationToken)
    {
        var lead = await _repository.GetByIdAsync(request.CompanyId, request.LeadId, cancellationToken)
            ?? throw new KeyNotFoundException($"Lead '{request.LeadId}' was not found.");

        return LeadMapper.ToDto(lead);
    }
}
