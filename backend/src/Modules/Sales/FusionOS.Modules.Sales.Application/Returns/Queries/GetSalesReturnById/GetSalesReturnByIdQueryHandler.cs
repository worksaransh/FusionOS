using FusionOS.Modules.Sales.Application.Returns.Commands.CreateSalesReturn;
using FusionOS.Modules.Sales.Application.Returns.Contracts;
using MediatR;

namespace FusionOS.Modules.Sales.Application.Returns.Queries.GetSalesReturnById;

public sealed class GetSalesReturnByIdQueryHandler : IRequestHandler<GetSalesReturnByIdQuery, SalesReturnDto>
{
    private readonly ISalesReturnRepository _repository;

    public GetSalesReturnByIdQueryHandler(ISalesReturnRepository repository) => _repository = repository;

    public async Task<SalesReturnDto> Handle(GetSalesReturnByIdQuery request, CancellationToken cancellationToken)
    {
        var salesReturn = await _repository.GetByIdAsync(request.CompanyId, request.SalesReturnId, cancellationToken)
            ?? throw new KeyNotFoundException($"Sales return '{request.SalesReturnId}' was not found.");

        return CreateSalesReturnCommandHandler.MapToDto(salesReturn);
    }
}
