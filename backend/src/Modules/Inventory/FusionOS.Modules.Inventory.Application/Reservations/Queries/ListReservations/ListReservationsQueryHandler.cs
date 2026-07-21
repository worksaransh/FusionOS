using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Inventory.Application.Reservations.Contracts;
using MediatR;

namespace FusionOS.Modules.Inventory.Application.Reservations.Queries.ListReservations;

public sealed class ListReservationsQueryHandler : IRequestHandler<ListReservationsQuery, PagedResult<ReservationDto>>
{
    private readonly IReservationRepository _repository;

    public ListReservationsQueryHandler(IReservationRepository repository) => _repository = repository;

    public async Task<PagedResult<ReservationDto>> Handle(ListReservationsQuery request, CancellationToken cancellationToken)
    {
        var reservations = await _repository.ListAsync(request.CompanyId, request.ProductId, request.WarehouseId, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, request.ProductId, request.WarehouseId, cancellationToken);

        var dtos = reservations.Select(ReservationMapper.ToDto).ToList();

        return new PagedResult<ReservationDto>(dtos, request.Page, request.PageSize, total);
    }
}
