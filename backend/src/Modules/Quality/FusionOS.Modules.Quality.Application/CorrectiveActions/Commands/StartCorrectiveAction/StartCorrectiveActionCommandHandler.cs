using FusionOS.Modules.Quality.Application.CorrectiveActions.Contracts;
using MediatR;

namespace FusionOS.Modules.Quality.Application.CorrectiveActions.Commands.StartCorrectiveAction;

public sealed class StartCorrectiveActionCommandHandler : IRequestHandler<StartCorrectiveActionCommand, CorrectiveActionDto>
{
    private readonly ICorrectiveActionRepository _repository;
    private readonly FusionOS.Modules.Quality.Application.Inspections.Contracts.IUnitOfWork _unitOfWork;

    public StartCorrectiveActionCommandHandler(ICorrectiveActionRepository repository, FusionOS.Modules.Quality.Application.Inspections.Contracts.IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CorrectiveActionDto> Handle(StartCorrectiveActionCommand request, CancellationToken cancellationToken)
    {
        var correctiveAction = await _repository.GetByIdAsync(request.CompanyId, request.CorrectiveActionId, cancellationToken)
            ?? throw new KeyNotFoundException($"Corrective action '{request.CorrectiveActionId}' was not found.");

        correctiveAction.Start();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CorrectiveActionMapper.ToDto(correctiveAction);
    }
}
