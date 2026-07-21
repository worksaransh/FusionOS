using FusionOS.Modules.Crm.Application.Contacts.Contracts;
using MediatR;

namespace FusionOS.Modules.Crm.Application.Contacts.Queries.GetContactById;

public sealed class GetContactByIdQueryHandler : IRequestHandler<GetContactByIdQuery, ContactDto>
{
    private readonly IContactRepository _repository;

    public GetContactByIdQueryHandler(IContactRepository repository) => _repository = repository;

    public async Task<ContactDto> Handle(GetContactByIdQuery request, CancellationToken cancellationToken)
    {
        var contact = await _repository.GetByIdAsync(request.CompanyId, request.ContactId, cancellationToken)
            ?? throw new KeyNotFoundException($"Contact '{request.ContactId}' was not found.");

        return ContactMapper.ToDto(contact);
    }
}
