using FusionOS.Modules.Quality.Application.Audits.Contracts;
using MediatR;

namespace FusionOS.Modules.Quality.Application.Audits.Commands.ResolveAuditFinding;

public sealed class ResolveAuditFindingCommandHandler : IRequestHandler<ResolveAuditFindingCommand, QualityAuditDto>
{
    private readonly IQualityAuditRepository _repository;
    private readonly FusionOS.Modules.Quality.Application.Inspections.Contracts.IUnitOfWork _unitOfWork;

    public ResolveAuditFindingCommandHandler(IQualityAuditRepository repository, FusionOS.Modules.Quality.Application.Inspections.Contracts.IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<QualityAuditDto> Handle(ResolveAuditFindingCommand request, CancellationToken cancellationToken)
    {
        var audit = await _repository.GetByIdAsync(request.CompanyId, request.QualityAuditId, cancellationToken)
            ?? throw new KeyNotFoundException($"Quality audit '{request.QualityAuditId}' was not found.");

        audit.ResolveFinding(request.FindingId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return QualityAuditMapper.ToDto(audit);
    }
}
