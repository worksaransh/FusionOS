using FusionOS.Modules.Crm.Application.Tags.Contracts;
using MediatR;

namespace FusionOS.Modules.Crm.Application.Tags.Queries.ListTagsForEntity;

public sealed class ListTagsForEntityQueryHandler : IRequestHandler<ListTagsForEntityQuery, IReadOnlyList<TagDto>>
{
    private readonly IEntityTagRepository _entityTagRepository;
    private readonly ITagRepository _tagRepository;

    public ListTagsForEntityQueryHandler(IEntityTagRepository entityTagRepository, ITagRepository tagRepository)
    {
        _entityTagRepository = entityTagRepository;
        _tagRepository = tagRepository;
    }

    public async Task<IReadOnlyList<TagDto>> Handle(ListTagsForEntityQuery request, CancellationToken cancellationToken)
    {
        var entityTags = await _entityTagRepository.ListForEntityAsync(request.CompanyId, request.EntityType, request.EntityId, cancellationToken);
        if (entityTags.Count == 0)
            return Array.Empty<TagDto>();

        var tagIds = entityTags.Select(et => et.TagId).Distinct().ToList();
        var tags = await _tagRepository.GetByIdsAsync(request.CompanyId, tagIds, cancellationToken);

        return tags.Select(TagMapper.ToDto).ToList();
    }
}
