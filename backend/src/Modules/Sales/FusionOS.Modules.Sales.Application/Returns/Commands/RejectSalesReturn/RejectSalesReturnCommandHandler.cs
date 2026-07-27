using FusionOS.Modules.Sales.Application.Customers.Contracts;
using FusionOS.Modules.Sales.Application.Returns.Commands.CreateSalesReturn;
using FusionOS.Modules.Sales.Application.Returns.Contracts;
using MediatR;

namespace FusionOS.Modules.Sales.Application.Returns.Commands.RejectSalesReturn;

public sealed class RejectSalesReturnCommandHandler : IRequestHandler<RejectSalesReturnCommand, SalesReturnDto>
{
    private readonly ISalesReturnRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public RejectSalesReturnCommandHandler(ISalesReturnRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<SalesReturnDto> Handle(RejectSalesReturnCommand request, CancellationToken cancellationToken)
    {
        var salesReturn = await _repository.GetByIdAsync(request.CompanyId, request.SalesReturnId, cancellationToken)
            ?? throw new KeyNotFoundException($"Sales return '{request.SalesReturnId}' was not found.");

        salesReturn.Reject(request.RejectionReason);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateSalesReturnCommandHandler.MapToDto(salesReturn);
    }
}
