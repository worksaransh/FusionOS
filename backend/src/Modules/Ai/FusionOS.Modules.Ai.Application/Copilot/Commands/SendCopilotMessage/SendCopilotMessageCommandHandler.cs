using FusionOS.Modules.Ai.Application.Copilot.Contracts;
using FusionOS.Modules.Ai.Application.Copilot.Retrieval;
using FusionOS.Modules.Ai.Application.Recommendations.Contracts;
using FusionOS.Modules.Ai.Domain.Copilot;
using MediatR;

namespace FusionOS.Modules.Ai.Application.Copilot.Commands.SendCopilotMessage;

/// <summary>
/// The actual "send a message, get a response" flow: append the user's turn, run the
/// retrieval step over existing, already-permission-gated FusionOS data, generate a reply
/// grounded in whatever that retrieval found, append the reply as the Copilot's turn, and
/// persist both in one unit of work. See <see cref="CopilotRetrievalStep"/> for the
/// retrieval step and <see cref="IChatResponseProvider"/> for why response generation is a
/// deterministic template today rather than a real LLM call.
/// </summary>
public sealed class SendCopilotMessageCommandHandler : IRequestHandler<SendCopilotMessageCommand, CopilotSessionDto>
{
    private readonly ICopilotSessionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CopilotRetrievalStep _retrievalStep;
    private readonly IChatResponseProvider _responseProvider;

    public SendCopilotMessageCommandHandler(
        ICopilotSessionRepository repository,
        IUnitOfWork unitOfWork,
        CopilotRetrievalStep retrievalStep,
        IChatResponseProvider responseProvider)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _retrievalStep = retrievalStep;
        _responseProvider = responseProvider;
    }

    public async Task<CopilotSessionDto> Handle(SendCopilotMessageCommand request, CancellationToken cancellationToken)
    {
        var session = await _repository.GetByIdAsync(request.CompanyId, request.SessionId, cancellationToken)
            ?? throw new KeyNotFoundException($"Copilot session '{request.SessionId}' was not found.");

        session.AddMessage(CopilotRole.User, request.Message);

        var retrieved = await _retrievalStep.ResolveAsync(request.CompanyId, request.Message, cancellationToken);
        var response = await _responseProvider.GenerateResponseAsync(request.Message, retrieved, cancellationToken);

        session.AddMessage(CopilotRole.Assistant, response);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CopilotSessionMapper.ToDto(session);
    }
}
