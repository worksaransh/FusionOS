using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Sales.Application.Customers.Contracts;
using MediatR;

namespace FusionOS.Modules.Sales.Application.Customers.Commands.CreateCustomer;

public sealed class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, CustomerDto>
{
    private readonly ICustomerRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCustomerCommandHandler(ICustomerRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CustomerDto> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        if (await _repository.CodeExistsAsync(request.CompanyId, request.Code, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.Code), $"Customer code '{request.Code}' already exists for this company."),
            });
        }

        var customer = Domain.Customers.Customer.Create(request.CompanyId, request.Name, request.Code, request.ContactEmail, request.CreditLimit);

        await _repository.AddAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CustomerDto(customer.Id, customer.Name, customer.Code, customer.ContactEmail, customer.CreditLimit, customer.IsActive, customer.CreatedAt);
    }
}
