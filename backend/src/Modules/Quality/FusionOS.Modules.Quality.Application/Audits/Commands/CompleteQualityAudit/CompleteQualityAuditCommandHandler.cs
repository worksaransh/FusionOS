using FusionOS.Modules.Quality.Application.Audits.Contracts;
using MediatR;

namespace FusionOS.Modules.Quality.Application.Audits.Commands.CompleteQualityAudit;

public sealed class CompleteQualityAuditCommandHandler : IRequestHandler<CompleteQualityAuditCommand, QualityAuditDto>
{
    private readonly IQualityAuditRepository _repository;
    private readonly FusionOS.Modules.Quality.Application.Inspections.Contracts.IUnitOfWork _unitOfWork;

    public CompleteQualityAuditCommandHandler(IQualityAuditRepository repository, FusionOS.Modules.Quality.Application.Inspections.Contracts.IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<QualityAuditDto> Handle(CompleteQualityAuditCommand request, CancellationToken cancellationToken)
    {
        var audit = await _repository.GetByIdAsync(request.CompanyId, request.QualityAuditId, cancellationToken)
            ?? throw new KeyNotFoundException($"Quality audit '{request.QualityAuditId}' was not found.");

        audit.Complete();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return QualityAuditMapper.ToDto(audit);
    }
}
