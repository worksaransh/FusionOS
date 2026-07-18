using FusionOS.Modules.Inventory.Application.Products.Contracts;
using FusionOS.Modules.Inventory.Application.Reservations.Contracts;
using MediatR;

namespace FusionOS.Modules.Inventory.Application.Reservations.Commands.CreateReservation;

public sealed class CreateReservationCommandHandler : IRequestHandler<CreateReservationCommand, ReservationDto>
{
    private readonly IReservationRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateReservationCommandHandler(IReservationRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ReservationDto> Handle(CreateReservationCommand request, CancellationToken cancellationToken)
    {
        var reservation = Domain.Reservations.Reservation.Create(
            request.CompanyId, request.ProductId, request.WarehouseId, request.Quantity, request.ReferenceType, request.ReferenceId);

        await _repository.AddAsync(reservation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ReservationMapper.ToDto(reservation);
    }
}
