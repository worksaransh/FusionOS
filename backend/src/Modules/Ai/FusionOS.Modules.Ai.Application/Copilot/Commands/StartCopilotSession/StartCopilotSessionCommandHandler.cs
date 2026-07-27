using FusionOS.Modules.Ai.Application.Copilot.Contracts;
using FusionOS.Modules.Ai.Application.Recommendations.Contracts;
using MediatR;

namespace FusionOS.Modules.Ai.Application.Copilot.Commands.StartCopilotSession;

public sealed class StartCopilotSessionCommandHandler : IRequestHandler<StartCopilotSessionCommand, CopilotSessionDto>
{
    private readonly ICopilotSessionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public StartCopilotSessionCommandHandler(ICopilotSessionRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CopilotSessionDto> Handle(StartCopilotSessionCommand request, CancellationToken cancellationToken)
    {
        var session = Domain.Copilot.CopilotSession.Start(request.CompanyId, request.UserId, request.Title);

        await _repository.AddAsync(session, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CopilotSessionMapper.ToDto(session);
    }
}
