using FusionOS.Modules.Procurement.Application.Suppliers.Contracts;
using FusionOS.Modules.Procurement.Application.VendorReturns.Contracts;
using MediatR;

namespace FusionOS.Modules.Procurement.Application.VendorReturns.Commands.CompleteVendorReturn;

public sealed class CompleteVendorReturnCommandHandler : IRequestHandler<CompleteVendorReturnCommand, VendorReturnDto>
{
    private readonly IVendorReturnRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CompleteVendorReturnCommandHandler(IVendorReturnRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<VendorReturnDto> Handle(CompleteVendorReturnCommand request, CancellationToken cancellationToken)
    {
        var vendorReturn = await _repository.GetByIdAsync(request.CompanyId, request.VendorReturnId, cancellationToken)
            ?? throw new KeyNotFoundException($"Vendor return '{request.VendorReturnId}' was not found.");

        vendorReturn.Complete();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return VendorReturnMapper.ToDto(vendorReturn);
    }
}
