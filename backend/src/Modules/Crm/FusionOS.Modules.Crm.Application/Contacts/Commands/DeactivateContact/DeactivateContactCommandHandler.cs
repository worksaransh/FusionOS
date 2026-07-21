using FusionOS.Modules.Crm.Application.Contacts.Contracts;
using FusionOS.Modules.Crm.Application.Leads.Contracts;
using MediatR;

namespace FusionOS.Modules.Crm.Application.Contacts.Commands.DeactivateContact;

public sealed class DeactivateContactCommandHandler : IRequestHandler<DeactivateContactCommand, ContactDto>
{
    private readonly IContactRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateContactCommandHandler(IContactRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ContactDto> Handle(DeactivateContactCommand request, CancellationToken cancellationToken)
    {
        var contact = await _repository.GetByIdAsync(request.CompanyId, request.ContactId, cancellationToken)
            ?? throw new KeyNotFoundException($"Contact '{request.ContactId}' was not found.");

        contact.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ContactMapper.ToDto(contact);
    }
}
