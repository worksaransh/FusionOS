using FusionOS.Modules.Maintenance.Application.ChecklistTemplates.Contracts;
using MediatR;

namespace FusionOS.Modules.Maintenance.Application.ChecklistTemplates.Queries.GetChecklistTemplateById;

public sealed class GetChecklistTemplateByIdQueryHandler : IRequestHandler<GetChecklistTemplateByIdQuery, ChecklistTemplateDto>
{
    private readonly IChecklistTemplateRepository _repository;

    public GetChecklistTemplateByIdQueryHandler(IChecklistTemplateRepository repository) => _repository = repository;

    public async Task<ChecklistTemplateDto> Handle(GetChecklistTemplateByIdQuery request, CancellationToken cancellationToken)
    {
        var template = await _repository.GetByIdAsync(request.CompanyId, request.ChecklistTemplateId, cancellationToken)
            ?? throw new KeyNotFoundException($"Checklist template '{request.ChecklistTemplateId}' was not found.");

        return ChecklistTemplateMapper.ToDto(template);
    }
}
