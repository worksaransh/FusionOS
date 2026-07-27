using FusionOS.Modules.Sales.Application.Customers.Contracts;
using FusionOS.Modules.Sales.Application.Returns.Commands.CreateSalesReturn;
using FusionOS.Modules.Sales.Application.Returns.Contracts;
using MediatR;

namespace FusionOS.Modules.Sales.Application.Returns.Commands.CompleteSalesReturn;

public sealed class CompleteSalesReturnCommandHandler : IRequestHandler<CompleteSalesReturnCommand, SalesReturnDto>
{
    private readonly ISalesReturnRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CompleteSalesReturnCommandHandler(ISalesReturnRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<SalesReturnDto> Handle(CompleteSalesReturnCommand request, CancellationToken cancellationToken)
    {
        var salesReturn = await _repository.GetByIdAsync(request.CompanyId, request.SalesReturnId, cancellationToken)
            ?? throw new KeyNotFoundException($"Sales return '{request.SalesReturnId}' was not found.");

        salesReturn.Complete(request.LineCompletions);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateSalesReturnCommandHandler.MapToDto(salesReturn);
    }
}
