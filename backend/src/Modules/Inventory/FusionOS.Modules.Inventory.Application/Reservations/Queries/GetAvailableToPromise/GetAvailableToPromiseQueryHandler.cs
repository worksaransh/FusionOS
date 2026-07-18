using FusionOS.Modules.Inventory.Application.Ledger.Contracts;
using FusionOS.Modules.Inventory.Application.Reservations.Contracts;
using MediatR;

namespace FusionOS.Modules.Inventory.Application.Reservations.Queries.GetAvailableToPromise;

public sealed class GetAvailableToPromiseQueryHandler : IRequestHandler<GetAvailableToPromiseQuery, AvailableToPromiseDto>
{
    private readonly IInventoryLedgerRepository _ledgerRepository;
    private readonly IReservationRepository _reservationRepository;

    public GetAvailableToPromiseQueryHandler(IInventoryLedgerRepository ledgerRepository, IReservationRepository reservationRepository)
    {
        _ledgerRepository = ledgerRepository;
        _reservationRepository = reservationRepository;
    }

    public async Task<AvailableToPromiseDto> Handle(GetAvailableToPromiseQuery request, CancellationToken cancellationToken)
    {
        var stockOnHand = await _ledgerRepository.SumQuantityAsync(request.CompanyId, request.ProductId, request.WarehouseId, cancellationToken);
        var reserved = await _reservationRepository.SumActiveQuantityAsync(request.CompanyId, request.ProductId, request.WarehouseId, cancellationToken);

        return new AvailableToPromiseDto(request.ProductId, request.WarehouseId, stockOnHand, reserved, stockOnHand - reserved);
    }
}
