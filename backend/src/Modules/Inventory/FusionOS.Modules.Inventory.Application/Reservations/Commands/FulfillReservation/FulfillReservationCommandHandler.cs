using FusionOS.Modules.Inventory.Application.Products.Contracts;
using FusionOS.Modules.Inventory.Application.Reservations.Contracts;
using MediatR;

namespace FusionOS.Modules.Inventory.Application.Reservations.Commands.FulfillReservation;

public sealed class FulfillReservationCommandHandler : IRequestHandler<FulfillReservationCommand, ReservationDto>
{
    private readonly IReservationRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public FulfillReservationCommandHandler(IReservationRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ReservationDto> Handle(FulfillReservationCommand request, CancellationToken cancellationToken)
    {
        var reservation = await _repository.GetByIdAsync(request.CompanyId, request.ReservationId, cancellationToken)
            ?? throw new KeyNotFoundException($"Reservation '{request.ReservationId}' was not found.");

        reservation.Fulfill();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ReservationMapper.ToDto(reservation);
    }
}
