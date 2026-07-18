using FusionOS.Modules.Sales.Application.Customers.Contracts;
using MediatR;

namespace FusionOS.Modules.Sales.Application.Customers.Commands.UpdateCustomer;

public sealed class UpdateCustomerCommandHandler : IRequestHandler<UpdateCustomerCommand, CustomerDto>
{
    private readonly ICustomerRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCustomerCommandHandler(ICustomerRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CustomerDto> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _repository.GetByIdAsync(request.CompanyId, request.CustomerId, cancellationToken)
            ?? throw new KeyNotFoundException($"Customer '{request.CustomerId}' was not found.");

        customer.UpdateDetails(request.Name, request.ContactEmail, request.CreditLimit);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CustomerDto(customer.Id, customer.Name, customer.Code, customer.ContactEmail, customer.CreditLimit, customer.IsActive, customer.CreatedAt, customer.PriceListId);
    }
}
