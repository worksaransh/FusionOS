using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Core.Application.Companies.Contracts;
using FusionOS.SharedKernel.Context;
using MediatR;

namespace FusionOS.Modules.Core.Application.Companies.Commands.SetCompanyParent;

public sealed class SetCompanyParentCommandHandler : IRequestHandler<SetCompanyParentCommand, CompanyDto?>
{
    private readonly ICompanyRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserContext _currentUser;

    public SetCompanyParentCommandHandler(ICompanyRepository repository, IUnitOfWork unitOfWork, ICurrentUserContext currentUser)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<CompanyDto?> Handle(SetCompanyParentCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.CompanyId is not { } companyId || companyId != request.Id)
            return null;

        var company = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (company is null)
            return null;

        if (request.ParentCompanyId is { } parentCompanyId)
        {
            var parent = await _repository.GetByIdAsync(parentCompanyId, cancellationToken);
            if (parent is null)
            {
                throw new ValidationException(new[]
                {
                    new FluentValidation.Results.ValidationFailure(nameof(request.ParentCompanyId), $"Parent company '{parentCompanyId}' was not found."),
                });
            }
        }

        company.SetParentCompany(request.ParentCompanyId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CompanyDto(company.Id, company.Name, company.LegalName, company.TaxId, company.BaseCurrency, company.IsActive, company.CreatedAt, company.ParentCompanyId);
    }
}
