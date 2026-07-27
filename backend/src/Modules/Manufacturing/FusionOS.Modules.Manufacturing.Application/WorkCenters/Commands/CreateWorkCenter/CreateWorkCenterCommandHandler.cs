using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Contracts;
using FusionOS.Modules.Manufacturing.Application.WorkCenters.Contracts;
using MediatR;

namespace FusionOS.Modules.Manufacturing.Application.WorkCenters.Commands.CreateWorkCenter;

public sealed class CreateWorkCenterCommandHandler : IRequestHandler<CreateWorkCenterCommand, WorkCenterDto>
{
    private readonly IWorkCenterRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateWorkCenterCommandHandler(IWorkCenterRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<WorkCenterDto> Handle(CreateWorkCenterCommand request, CancellationToken cancellationToken)
    {
        var code = request.Code?.Trim().ToUpperInvariant() ?? string.Empty;
        if (await _repository.CodeExistsAsync(request.CompanyId, code, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.Code), $"A work center with code '{code}' already exists for this company."),
            });
        }

        var workCenter = Domain.WorkCenters.WorkCenter.Create(request.CompanyId, request.Code, request.Name, request.CapacityHoursPerDay);

        await _repository.AddAsync(workCenter, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return WorkCenterMapper.ToDto(workCenter);
    }
}
