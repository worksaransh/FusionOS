using FusionOS.Modules.Maintenance.Application.Assets.Contracts;
using FusionOS.Modules.Maintenance.Application.ChecklistTemplates.Contracts;
using MediatR;

namespace FusionOS.Modules.Maintenance.Application.ChecklistTemplates.Commands.CreateChecklistTemplate;

public sealed class CreateChecklistTemplateCommandHandler : IRequestHandler<CreateChecklistTemplateCommand, ChecklistTemplateDto>
{
    private readonly IChecklistTemplateRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateChecklistTemplateCommandHandler(IChecklistTemplateRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ChecklistTemplateDto> Handle(CreateChecklistTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = Domain.ChecklistTemplates.ChecklistTemplate.Create(request.CompanyId, request.Name, request.Description, request.StepDescriptions);

        await _repository.AddAsync(template, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ChecklistTemplateMapper.ToDto(template);
    }
}
