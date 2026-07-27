using FusionOS.Modules.Maintenance.Application.Assets.Contracts;
using FusionOS.Modules.Maintenance.Application.ChecklistTemplates.Contracts;
using MediatR;

namespace FusionOS.Modules.Maintenance.Application.ChecklistTemplates.Commands.DeactivateChecklistTemplate;

public sealed class DeactivateChecklistTemplateCommandHandler : IRequestHandler<DeactivateChecklistTemplateCommand, ChecklistTemplateDto>
{
    private readonly IChecklistTemplateRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateChecklistTemplateCommandHandler(IChecklistTemplateRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ChecklistTemplateDto> Handle(DeactivateChecklistTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await _repository.GetByIdAsync(request.CompanyId, request.ChecklistTemplateId, cancellationToken)
            ?? throw new KeyNotFoundException($"Checklist template '{request.ChecklistTemplateId}' was not found.");

        template.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ChecklistTemplateMapper.ToDto(template);
    }
}
