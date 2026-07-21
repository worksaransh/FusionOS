using FusionOS.Modules.Quality.Application.CorrectiveActions.Contracts;
using MediatR;

namespace FusionOS.Modules.Quality.Application.CorrectiveActions.Commands.VerifyCorrectiveAction;

public sealed class VerifyCorrectiveActionCommandHandler : IRequestHandler<VerifyCorrectiveActionCommand, CorrectiveActionDto>
{
    private readonly ICorrectiveActionRepository _repository;
    private readonly FusionOS.Modules.Quality.Application.Inspections.Contracts.IUnitOfWork _unitOfWork;

    public VerifyCorrectiveActionCommandHandler(ICorrectiveActionRepository repository, FusionOS.Modules.Quality.Application.Inspections.Contracts.IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CorrectiveActionDto> Handle(VerifyCorrectiveActionCommand request, CancellationToken cancellationToken)
    {
        var correctiveAction = await _repository.GetByIdAsync(request.CompanyId, request.CorrectiveActionId, cancellationToken)
            ?? throw new KeyNotFoundException($"Corrective action '{request.CorrectiveActionId}' was not found.");

        correctiveAction.Verify();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CorrectiveActionMapper.ToDto(correctiveAction);
    }
}
