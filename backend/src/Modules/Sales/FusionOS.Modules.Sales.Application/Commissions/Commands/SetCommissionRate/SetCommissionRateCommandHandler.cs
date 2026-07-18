using FusionOS.Modules.Sales.Application.Commissions.Contracts;
using FusionOS.Modules.Sales.Application.Customers.Contracts;
using MediatR;

namespace FusionOS.Modules.Sales.Application.Commissions.Commands.SetCommissionRate;

/// <summary>Get-or-create/upsert, same restraint as CompanySettings' get-or-create pattern (Phase 5) — one rate row per (CompanyId, UserId), never more.</summary>
public sealed class SetCommissionRateCommandHandler : IRequestHandler<SetCommissionRateCommand, SalesCommissionRateDto>
{
    private readonly ISalesCommissionRateRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public SetCommissionRateCommandHandler(ISalesCommissionRateRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<SalesCommissionRateDto> Handle(SetCommissionRateCommand request, CancellationToken cancellationToken)
    {
        var existing = await _repository.GetByUserIdAsync(request.CompanyId, request.UserId, cancellationToken);

        if (existing is not null)
        {
            existing.SetRate(request.RatePercentage);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return MapToDto(existing);
        }

        var rate = Domain.Commissions.SalesCommissionRate.Create(request.CompanyId, request.UserId, request.RatePercentage);
        await _repository.AddAsync(rate, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return MapToDto(rate);
    }

    internal static SalesCommissionRateDto MapToDto(Domain.Commissions.SalesCommissionRate rate) => new(rate.Id, rate.UserId, rate.RatePercentage);
}
