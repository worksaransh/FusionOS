using FusionOS.Modules.Crm.Application.Leads.Contracts;
using FusionOS.Modules.Crm.Application.Tags.Contracts;
using MediatR;

namespace FusionOS.Modules.Crm.Application.Tags.Commands.DetachTagFromEntity;

public sealed class DetachTagFromEntityCommandHandler : IRequestHandler<DetachTagFromEntityCommand, Unit>
{
    private readonly IEntityTagRepository _entityTagRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DetachTagFromEntityCommandHandler(IEntityTagRepository entityTagRepository, IUnitOfWork unitOfWork)
    {
        _entityTagRepository = entityTagRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(DetachTagFromEntityCommand request, CancellationToken cancellationToken)
    {
        var entityTag = await _entityTagRepository.FindAsync(request.CompanyId, request.TagId, request.TargetEntityType, request.TargetEntityId, cancellationToken)
            ?? throw new KeyNotFoundException("This tag is not attached to the given entity.");

        _entityTagRepository.Remove(entityTag);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
