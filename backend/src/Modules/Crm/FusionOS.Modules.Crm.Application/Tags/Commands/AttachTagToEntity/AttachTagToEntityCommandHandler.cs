using FusionOS.Modules.Crm.Application.Leads.Contracts;
using FusionOS.Modules.Crm.Application.Tags.Contracts;
using MediatR;

namespace FusionOS.Modules.Crm.Application.Tags.Commands.AttachTagToEntity;

public sealed class AttachTagToEntityCommandHandler : IRequestHandler<AttachTagToEntityCommand, TagAttachmentDto>
{
    private readonly ITagRepository _tagRepository;
    private readonly IEntityTagRepository _entityTagRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AttachTagToEntityCommandHandler(ITagRepository tagRepository, IEntityTagRepository entityTagRepository, IUnitOfWork unitOfWork)
    {
        _tagRepository = tagRepository;
        _entityTagRepository = entityTagRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<TagAttachmentDto> Handle(AttachTagToEntityCommand request, CancellationToken cancellationToken)
    {
        if (!await _tagRepository.ExistsAsync(request.CompanyId, request.TagId, cancellationToken))
            throw new KeyNotFoundException($"Tag '{request.TagId}' was not found.");

        var existing = await _entityTagRepository.FindAsync(request.CompanyId, request.TagId, request.TargetEntityType, request.TargetEntityId, cancellationToken);
        if (existing is not null)
            return EntityTagMapper.ToDto(existing);

        var entityTag = Domain.Tags.EntityTag.Create(request.CompanyId, request.TagId, request.TargetEntityType, request.TargetEntityId);

        await _entityTagRepository.AddAsync(entityTag, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return EntityTagMapper.ToDto(entityTag);
    }
}
