using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Maintenance.Application.ChecklistTemplates.Contracts;
using MediatR;

namespace FusionOS.Modules.Maintenance.Application.ChecklistTemplates.Queries.ListChecklistTemplates;

public sealed class ListChecklistTemplatesQueryHandler : IRequestHandler<ListChecklistTemplatesQuery, PagedResult<ChecklistTemplateDto>>
{
    private readonly IChecklistTemplateRepository _repository;

    public ListChecklistTemplatesQueryHandler(IChecklistTemplateRepository repository) => _repository = repository;

    public async Task<PagedResult<ChecklistTemplateDto>> Handle(ListChecklistTemplatesQuery request, CancellationToken cancellationToken)
    {
        var templates = await _repository.ListAsync(request.CompanyId, request.Search, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, request.Search, cancellationToken);

        var dtos = templates.Select(ChecklistTemplateMapper.ToDto).ToList();

        return new PagedResult<ChecklistTemplateDto>(dtos, request.Page, request.PageSize, total);
    }
}
