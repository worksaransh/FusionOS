using FusionOS.Modules.Sales.Application.Customers.Contracts;
using FusionOS.Modules.Sales.Application.Returns.Commands.CreateSalesReturn;
using FusionOS.Modules.Sales.Application.Returns.Contracts;
using MediatR;

namespace FusionOS.Modules.Sales.Application.Returns.Commands.ApproveSalesReturn;

public sealed class ApproveSalesReturnCommandHandler : IRequestHandler<ApproveSalesReturnCommand, SalesReturnDto>
{
    private readonly ISalesReturnRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public ApproveSalesReturnCommandHandler(ISalesReturnRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<SalesReturnDto> Handle(ApproveSalesReturnCommand request, CancellationToken cancellationToken)
    {
        var salesReturn = await _repository.GetByIdAsync(request.CompanyId, request.SalesReturnId, cancellationToken)
            ?? throw new KeyNotFoundException($"Sales return '{request.SalesReturnId}' was not found.");

        salesReturn.Approve();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateSalesReturnCommandHandler.MapToDto(salesReturn);
    }
}
