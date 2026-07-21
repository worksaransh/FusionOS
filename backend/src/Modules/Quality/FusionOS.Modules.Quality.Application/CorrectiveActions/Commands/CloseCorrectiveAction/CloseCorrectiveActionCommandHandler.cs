using FusionOS.Modules.Quality.Application.CorrectiveActions.Contracts;
using MediatR;

namespace FusionOS.Modules.Quality.Application.CorrectiveActions.Commands.CloseCorrectiveAction;

public sealed class CloseCorrectiveActionCommandHandler : IRequestHandler<CloseCorrectiveActionCommand, CorrectiveActionDto>
{
    private readonly ICorrectiveActionRepository _repository;
    private readonly FusionOS.Modules.Quality.Application.Inspections.Contracts.IUnitOfWork _unitOfWork;

    public CloseCorrectiveActionCommandHandler(ICorrectiveActionRepository repository, FusionOS.Modules.Quality.Application.Inspections.Contracts.IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CorrectiveActionDto> Handle(CloseCorrectiveActionCommand request, CancellationToken cancellationToken)
    {
        var correctiveAction = await _repository.GetByIdAsync(request.CompanyId, request.CorrectiveActionId, cancellationToken)
            ?? throw new KeyNotFoundException($"Corrective action '{request.CorrectiveActionId}' was not found.");

        correctiveAction.Close();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CorrectiveActionMapper.ToDto(correctiveAction);
    }
}
