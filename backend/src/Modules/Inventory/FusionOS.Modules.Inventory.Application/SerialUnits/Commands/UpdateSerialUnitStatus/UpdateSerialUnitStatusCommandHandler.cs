using FusionOS.Modules.Inventory.Application.Products.Contracts;
using FusionOS.Modules.Inventory.Application.SerialUnits.Contracts;
using FusionOS.Modules.Inventory.Domain.SerialUnits;
using MediatR;

namespace FusionOS.Modules.Inventory.Application.SerialUnits.Commands.UpdateSerialUnitStatus;

public sealed class UpdateSerialUnitStatusCommandHandler : IRequestHandler<UpdateSerialUnitStatusCommand, SerialUnitDto>
{
    private readonly ISerialUnitRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateSerialUnitStatusCommandHandler(ISerialUnitRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<SerialUnitDto> Handle(UpdateSerialUnitStatusCommand request, CancellationToken cancellationToken)
    {
        var unit = await _repository.GetByIdAsync(request.CompanyId, request.SerialUnitId, cancellationToken)
            ?? throw new KeyNotFoundException($"Serial unit '{request.SerialUnitId}' was not found.");

        switch (request.NewStatus)
        {
            case SerialUnitStatus.Reserved:
                unit.MarkReserved();
                break;
            case SerialUnitStatus.Sold:
                unit.MarkSold();
                break;
            case SerialUnitStatus.Returned:
                unit.MarkReturned();
                break;
            case SerialUnitStatus.Defective:
                unit.MarkDefective();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(request.NewStatus), request.NewStatus, "Not a valid target status.");
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return SerialUnitMapper.ToDto(unit);
    }
}
