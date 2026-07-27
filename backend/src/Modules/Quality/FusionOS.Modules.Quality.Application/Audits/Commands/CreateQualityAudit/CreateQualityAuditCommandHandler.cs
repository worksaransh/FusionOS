using FusionOS.Modules.Quality.Application.Audits.Contracts;
using MediatR;

namespace FusionOS.Modules.Quality.Application.Audits.Commands.CreateQualityAudit;

public sealed class CreateQualityAuditCommandHandler : IRequestHandler<CreateQualityAuditCommand, QualityAuditDto>
{
    private readonly IQualityAuditRepository _repository;
    private readonly FusionOS.Modules.Quality.Application.Inspections.Contracts.IUnitOfWork _unitOfWork;

    public CreateQualityAuditCommandHandler(IQualityAuditRepository repository, FusionOS.Modules.Quality.Application.Inspections.Contracts.IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<QualityAuditDto> Handle(CreateQualityAuditCommand request, CancellationToken cancellationToken)
    {
        var audit = Domain.Audits.QualityAudit.Create(request.CompanyId, request.AuditType, request.ScheduledDate, request.Auditor, request.Scope);

        await _repository.AddAsync(audit, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return QualityAuditMapper.ToDto(audit);
    }
}
