using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Sales.Application.Customers.Contracts;
using FusionOS.Modules.Sales.Application.PortalUsers.Contracts;
using FusionOS.Modules.Sales.Domain.PortalUsers;
using MediatR;

namespace FusionOS.Modules.Sales.Application.PortalUsers.Commands.CreateCustomerPortalUser;

public sealed class CreateCustomerPortalUserCommandHandler : IRequestHandler<CreateCustomerPortalUserCommand, CreateCustomerPortalUserResult>
{
    private readonly ICustomerPortalUserRepository _portalUserRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IPortalPasswordHasher _hasher;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCustomerPortalUserCommandHandler(
        ICustomerPortalUserRepository portalUserRepository,
        ICustomerRepository customerRepository,
        IPortalPasswordHasher hasher,
        IUnitOfWork unitOfWork)
    {
        _portalUserRepository = portalUserRepository;
        _customerRepository = customerRepository;
        _hasher = hasher;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateCustomerPortalUserResult> Handle(CreateCustomerPortalUserCommand request, CancellationToken cancellationToken)
    {
        var customerExists = await _customerRepository.ExistsAsync(request.CompanyId, request.CustomerId, cancellationToken);
        if (!customerExists)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.CustomerId), "Customer does not exist for this company."),
            });
        }

        var emailInUse = await _portalUserRepository.ExistsForCustomerAsync(request.CustomerId, request.Email, cancellationToken);
        if (emailInUse)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.Email), "This customer already has a portal user with that email."),
            });
        }

        var passwordHash = _hasher.Hash(request.Password);
        var portalUser = CustomerPortalUser.Create(request.CompanyId, request.CustomerId, request.Email, passwordHash);

        await _portalUserRepository.AddAsync(portalUser, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateCustomerPortalUserResult(portalUser.Id, portalUser.CustomerId, portalUser.Email);
    }
}
