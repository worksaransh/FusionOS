using FusionOS.Modules.Procurement.Application.Suppliers.Contracts;
using FusionOS.Modules.Procurement.Application.VendorReturns.Contracts;
using MediatR;

namespace FusionOS.Modules.Procurement.Application.VendorReturns.Commands.CancelVendorReturn;

public sealed class CancelVendorReturnCommandHandler : IRequestHandler<CancelVendorReturnCommand, VendorReturnDto>
{
    private readonly IVendorReturnRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CancelVendorReturnCommandHandler(IVendorReturnRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<VendorReturnDto> Handle(CancelVendorReturnCommand request, CancellationToken cancellationToken)
    {
        var vendorReturn = await _repository.GetByIdAsync(request.CompanyId, request.VendorReturnId, cancellationToken)
            ?? throw new KeyNotFoundException($"Vendor return '{request.VendorReturnId}' was not found.");

        vendorReturn.Cancel();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return VendorReturnMapper.ToDto(vendorReturn);
    }
}
