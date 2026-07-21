using FusionOS.Modules.Core.Application.Comments.Contracts;
using MediatR;

namespace FusionOS.Modules.Core.Application.Comments.Queries.ListCommentsByEntity;

public sealed class ListCommentsByEntityQueryHandler : IRequestHandler<ListCommentsByEntityQuery, IReadOnlyList<CommentDto>>
{
    private readonly ICommentRepository _repository;

    public ListCommentsByEntityQueryHandler(ICommentRepository repository) => _repository = repository;

    public Task<IReadOnlyList<CommentDto>> Handle(ListCommentsByEntityQuery request, CancellationToken cancellationToken) =>
        _repository.ListByEntityAsync(request.CompanyId, request.EntityType, request.EntityId, cancellationToken);
}
