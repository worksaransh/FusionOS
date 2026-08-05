using FusionOS.Modules.Maintenance.Application.Assets.Contracts;
using FusionOS.Modules.Maintenance.Application.ChecklistTemplates.Contracts;
using MediatR;

namespace FusionOS.Modules.Maintenance.Application.ChecklistTemplates.Commands.UpdateChecklistTemplate;

public sealed class UpdateChecklistTemplateCommandHandler : IRequestHandler<UpdateChecklistTemplateCommand, ChecklistTemplateDto>
{
    private readonly IChecklistTemplateRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateChecklistTemplateCommandHandler(IChecklistTemplateRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ChecklistTemplateDto> Handle(UpdateChecklistTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await _repository.GetByIdAsync(request.CompanyId, request.ChecklistTemplateId, cancellationToken)
            ?? throw new KeyNotFoundException($"Checklist template '{request.ChecklistTemplateId}' was not found.");

        template.UpdateDetails(request.Name, request.Description, request.StepDescriptions);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ChecklistTemplateMapper.ToDto(template);
    }
}
