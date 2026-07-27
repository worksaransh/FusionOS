using FusionOS.Modules.Inventory.Application.Ledger.Contracts;
using FusionOS.Modules.Inventory.Application.Products.Contracts;
using FusionOS.Modules.Inventory.Application.Reservations.Contracts;
using MediatR;

namespace FusionOS.Modules.Inventory.Application.Reservations.Commands.CreateReservation;

public sealed class CreateReservationCommandHandler : IRequestHandler<CreateReservationCommand, ReservationDto>
{
    private readonly IReservationRepository _repository;
    private readonly IInventoryLedgerRepository _ledgerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateReservationCommandHandler(IReservationRepository repository, IInventoryLedgerRepository ledgerRepository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _ledgerRepository = ledgerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ReservationDto> Handle(CreateReservationCommand request, CancellationToken cancellationToken)
    {
        // Over-reservation guard (Phase 10 audit fix): mirrors
        // GetAvailableToPromiseQueryHandler's ATP = StockOnHand - sum(Active
        // reservations) calculation, since a reservation must never be created
        // for more than what's actually available to promise for this
        // Product/Warehouse. Computed here directly against the two source
        // repositories (rather than via IMediator.Send(GetAvailableToPromiseQuery))
        // to avoid re-running that query's own permission check inside this
        // command's handler.
        var stockOnHand = await _ledgerRepository.SumQuantityAsync(request.CompanyId, request.ProductId, request.WarehouseId, cancellationToken);
        var alreadyReserved = await _repository.SumActiveQuantityAsync(request.CompanyId, request.ProductId, request.WarehouseId, cancellationToken);
        var availableToPromise = stockOnHand - alreadyReserved;

        if (request.Quantity > availableToPromise)
        {
            throw new InvalidOperationException(
                $"Product {request.ProductId}: reserving {request.Quantity} would exceed the available-to-promise quantity for warehouse {request.WarehouseId} " +
                $"({availableToPromise} available, {stockOnHand} on hand, {alreadyReserved} already reserved).");
        }

        var reservation = Domain.Reservations.Reservation.Create(
            request.CompanyId, request.ProductId, request.WarehouseId, request.Quantity, request.ReferenceType, request.ReferenceId);

        await _repository.AddAsync(reservation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ReservationMapper.ToDto(reservation);
    }
}
