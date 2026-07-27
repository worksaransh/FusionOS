using FusionOS.Modules.Quality.Application.Audits.Contracts;
using MediatR;

namespace FusionOS.Modules.Quality.Application.Audits.Queries.GetQualityAuditById;

public sealed class GetQualityAuditByIdQueryHandler : IRequestHandler<GetQualityAuditByIdQuery, QualityAuditDto>
{
    private readonly IQualityAuditRepository _repository;

    public GetQualityAuditByIdQueryHandler(IQualityAuditRepository repository) => _repository = repository;

    public async Task<QualityAuditDto> Handle(GetQualityAuditByIdQuery request, CancellationToken cancellationToken)
    {
        var audit = await _repository.GetByIdAsync(request.CompanyId, request.QualityAuditId, cancellationToken)
            ?? throw new KeyNotFoundException($"Quality audit '{request.QualityAuditId}' was not found.");

        return QualityAuditMapper.ToDto(audit);
    }
}
