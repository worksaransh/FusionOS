using FusionOS.Modules.Warehouse.Application.Racks.Contracts;
using MediatR;

namespace FusionOS.Modules.Warehouse.Application.Racks.Queries.GetRackById;

public sealed class GetRackByIdQueryHandler : IRequestHandler<GetRackByIdQuery, RackDto?>
{
    private readonly IRackRepository _repository;

    public GetRackByIdQueryHandler(IRackRepository repository) => _repository = repository;

    public async Task<RackDto?> Handle(GetRackByIdQuery request, CancellationToken cancellationToken)
    {
        var rack = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (rack is null || rack.CompanyId != request.CompanyId)
            return null;

        return new RackDto(rack.Id, rack.ZoneId, rack.Name, rack.Code, rack.IsActive, rack.CreatedAt);
    }
}
