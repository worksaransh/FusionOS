using FusionOS.Modules.Sales.Application.Customers.Contracts;
using FusionOS.Modules.Sales.Application.PriceLists.Contracts;
using MediatR;

namespace FusionOS.Modules.Sales.Application.PriceLists.Commands.CreatePriceList;

public sealed class CreatePriceListCommandHandler : IRequestHandler<CreatePriceListCommand, PriceListDto>
{
    private readonly IPriceListRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePriceListCommandHandler(IPriceListRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PriceListDto> Handle(CreatePriceListCommand request, CancellationToken cancellationToken)
    {
        var priceList = Domain.PriceLists.PriceList.Create(request.CompanyId, request.Name, request.Entries);

        await _repository.AddAsync(priceList, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(priceList);
    }

    internal static PriceListDto MapToDto(Domain.PriceLists.PriceList priceList) => new(
        priceList.Id,
        priceList.Name,
        priceList.IsActive,
        priceList.Entries.Select(e => new PriceListEntryDto(e.Id, e.ProductId, e.UnitPrice)).ToList());
}
