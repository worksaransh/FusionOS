using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Crm.Application.Tags.Contracts;
using MediatR;

namespace FusionOS.Modules.Crm.Application.Tags.Queries.ListTags;

public sealed class ListTagsQueryHandler : IRequestHandler<ListTagsQuery, PagedResult<TagDto>>
{
    private readonly ITagRepository _repository;

    public ListTagsQueryHandler(ITagRepository repository) => _repository = repository;

    public async Task<PagedResult<TagDto>> Handle(ListTagsQuery request, CancellationToken cancellationToken)
    {
        var tags = await _repository.ListAsync(request.CompanyId, request.Search, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, request.Search, cancellationToken);

        var dtos = tags.Select(TagMapper.ToDto).ToList();

        return new PagedResult<TagDto>(dtos, request.Page, request.PageSize, total);
    }
}
